using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyResort.Data;
using QuanLyResort.Filters;
using QuanLyResort.Hubs;
using QuanLyResort.Middleware;
using QuanLyResort.Repositories;
using QuanLyResort.Services;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context
builder.Services.AddDbContext<ResortDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use SQLite if connection string is SQLite format, otherwise try SQL Server
    // SQLite works on all platforms (Windows, Linux, macOS)
    if (connectionString != null && (connectionString.Contains("Data Source=") || connectionString.Contains(".db")))
    {
        options.UseSqlite(connectionString);
    }
    else if (builder.Environment.IsDevelopment())
    {
        // Development: prefer SQLite for cross-platform
        options.UseSqlite(connectionString ?? "Data Source=ResortDev.db");
    }
    else
    {
        // Production: try SQL Server, but fallback to SQLite if LocalDB (not supported on Linux)
        if (connectionString != null && connectionString.Contains("(localdb)"))
        {
            // LocalDB not supported on Linux (Render), use SQLite instead
            options.UseSqlite("Data Source=resort.db");
        }
        else
        {
            options.UseSqlServer(connectionString);
        }
    }
});

// Add Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add HttpContextAccessor for AuditService to capture IP and UserAgent
builder.Services.AddHttpContextAccessor();

// Add Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IExternalDeviceService, DummyExternalDeviceService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPaymentSessionService, PaymentSessionService>();
builder.Services.AddScoped<IBankWebhookService, BankWebhookService>();
builder.Services.AddScoped<VietQRWebhookService>();
builder.Services.AddScoped<MBBankWebhookService>();
builder.Services.AddScoped<MBBankApiService>();
builder.Services.AddScoped<PayOsWebhookService>();
builder.Services.AddHttpClient();

// Add SignalR
builder.Services.AddSignalR();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    
    // Cấu hình JWT cho SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR gửi token qua query string hoặc header
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws/payment"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie("Cookies", options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// Add CORS - allow common local dev origins used by frontend (adjust as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevAllow", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5130",
                "https://localhost:5130",
                "http://127.0.0.1:5130",
                "http://localhost:5140",
                "https://localhost:5140",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // nếu frontend gửi cookie/credentials
    });
    // Optional permissive policy for quick testing (do NOT use in production)
    options.AddPolicy("DevAllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Controllers
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Add Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Resort Management API",
        Version = "v1",
        Description = "Complete Resort Management System API with JWT Authentication"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Ignore obsolete actions and use full type names to avoid conflicts
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    
    // Handle circular references
    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();
    
    // Exclude endpoints with IFormFile parameters to avoid Swagger generation errors
    // These endpoints work fine in runtime but Swagger can't generate docs for them properly
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        try
        {
            // Check by action descriptor display name (more reliable)
            var displayName = apiDesc.ActionDescriptor.DisplayName ?? "";
            var problematicActions = new[] { "UploadRoomImage", "AddImageToGallery", "UploadServiceImage" };
            
            foreach (var problematicAction in problematicActions)
            {
                if (displayName.Contains(problematicAction))
                {
                    return false; // Exclude these actions
                }
            }
            
            // Also check by relative path
            var relativePath = apiDesc.RelativePath ?? "";
            if (relativePath.Contains("UploadRoomImage") || 
                relativePath.Contains("AddImageToGallery") || 
                relativePath.Contains("UploadServiceImage"))
            {
                return false;
            }
            
            // Check for IFormFile parameters (backup check)
            var hasIFormFile = apiDesc.ParameterDescriptions.Any(p =>
            {
                try
                {
                    return p.Type == typeof(IFormFile) ||
                           (p.Type.IsGenericType && 
                            p.Type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                            p.Type.GetGenericArguments()[0] == typeof(IFormFile));
                }
                catch
                {
                    return false;
                }
            });
            
            return !hasIFormFile;
        }
        catch
        {
            // If we can't check, include the endpoint (better to show than hide)
            return true;
        }
    });
});

var app = builder.Build();

// Seed initial data and ensure database exists in Development
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ResortDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await context.Database.EnsureCreatedAsync();
    }
    var seeder = new DataSeeder(context);
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Add exception handling for Swagger generation
    // If Swagger generation fails, return a minimal valid Swagger document instead of 500
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger") && 
            context.Request.Path.Value?.Contains("/swagger.json") == true)
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                
                // Log full exception details including inner exception
                logger.LogError(ex, "Error generating Swagger documentation");
                
                if (ex.InnerException != null)
                {
                    logger.LogError(ex.InnerException, "Inner exception details");
                }
                
                // Return a minimal valid Swagger document instead of error
                // This allows Swagger UI to load even if some endpoints fail
                var minimalSwagger = new
                {
                    openapi = "3.0.1",
                    info = new
                    {
                        title = "Resort Management API",
                        version = "v1",
                        description = "API documentation (some endpoints excluded due to file upload support)"
                    },
                    paths = new { },
                    components = new { }
                };
                
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(minimalSwagger);
            }
        }
        else
        {
            await next();
        }
    });
    
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
        c.SerializeAsV2 = false; // Use OpenAPI 3.0
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resort Management API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

// app.UseHttpsRedirection(); // <- comment out only for local debug if needed

app.UseCors("LocalDevAllow");

app.UseAuthentication();

// Custom JWT Authorization Middleware - Kiểm tra role và phân quyền
// Đặt TRƯỚC UseAuthorization() để có thể bypass authentication cho public endpoints
app.UseJwtAuthorizationMiddleware();

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<QuanLyResort.Hubs.PaymentHub>("/ws/payment");

// Serve static files from wwwroot and ClientApp
app.UseStaticFiles();

app.Run();
