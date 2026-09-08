using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace LoadTestTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string choice = args.Length > 0 ? args[0] : "";
            if (string.IsNullOrEmpty(choice))
            {
                Console.WriteLine("=== Resort Management Load Test & Data Gen ===");
                Console.WriteLine("1. Generate 1,000,000 Bookings in SQL Server");
                Console.WriteLine("2. Run Load Test (GET /api/bookings)");
                Console.Write("Select option (1 or 2): ");
                choice = Console.ReadLine();
            }

            if (choice == "1")
            {
                await GenerateDataAsync();
            }
            else if (choice == "2")
            {
                await RunLoadTestAsync();
            }
        }

        static async Task GenerateDataAsync()
        {
            string masterConnString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;Encrypt=False";
            using (var masterConn = new SqlConnection(masterConnString))
            {
                await masterConn.OpenAsync();
                var createDbCmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ResortManagementDev2')
                    BEGIN
                        CREATE DATABASE ResortManagementDev2;
                    END
                ", masterConn);
                await createDbCmd.ExecuteNonQueryAsync();
            }

            string connString = "Server=(localdb)\\mssqllocaldb;Database=ResortManagementDev2;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";
            Console.WriteLine("Connecting to Database...");
            
            using var conn = new SqlConnection(connString);
            await conn.OpenAsync();
            
            Console.WriteLine("Generating Data...");
            string sql = @"
            SET NOCOUNT ON;

            IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerId = 1)
            BEGIN
                SET IDENTITY_INSERT Customers ON;
                INSERT INTO Customers (CustomerId, FullName, Email, PhoneNumber, CustomerType, CreatedAt, IsDeleted, TotalSpent, LoyaltyPoints)
                VALUES (1, 'Load Test User', 'test@example.com', '0123456789', 'Regular', GETUTCDATE(), 0, 0, 0);
                SET IDENTITY_INSERT Customers OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM Rooms)
            BEGIN
                INSERT INTO Rooms (RoomNumber, RoomType, IsAvailable, HousekeepingStatus, PricePerNight, MaxOccupancy, CreatedAt, Description, IsDeleted)
                VALUES 
                ('101', 'Standard', 1, 'Clean', 100, 2, GETUTCDATE(), 'Standard Room 1', 0),
                ('102', 'Deluxe', 1, 'Clean', 200, 2, GETUTCDATE(), 'Deluxe Room 2', 0),
                ('103', 'Suite', 1, 'Clean', 500, 4, GETUTCDATE(), 'Suite Room 3', 0),
                ('104', 'Villa', 1, 'Clean', 1000, 8, GETUTCDATE(), 'Villa Room 4', 0);
            END

            -- Create Customers
            ;WITH 
              E1(N) AS (SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1),
              E2(N) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b),
              E4(N) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b),
              Tally(N) AS (SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM E4)
            INSERT INTO Customers (FullName, Email, PhoneNumber, CustomerType, CreatedAt, IsDeleted, TotalSpent, LoyaltyPoints)
            SELECT 
                'Customer ' + CAST(N AS VARCHAR(10)),
                'customer' + CAST(N AS VARCHAR(10)) + '@example.com',
                '555-' + RIGHT('0000' + CAST(N AS VARCHAR(10)), 4),
                'Regular',
                GETUTCDATE(),
                0,
                0,
                0
            FROM Tally;

            DECLARE @CustomerId INT = (SELECT TOP 1 CustomerId FROM Customers);
            DECLARE @RoomId INT = (SELECT TOP 1 RoomId FROM Rooms);

            PRINT 'Starting bulk insert...';
            
            ;WITH 
              E1(N) AS (SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1),
              E2(N) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b),
              E4(N) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b),
              E6(N) AS (SELECT 1 FROM E4 a CROSS JOIN E2 b),
              Tally(N) AS (SELECT TOP 200000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) FROM E6)
            INSERT INTO Bookings (BookingCode, CustomerId, RoomId, RequestedRoomType, CheckInDate, CheckOutDate, NumberOfGuests, Status, EstimatedTotalAmount, BookedPrice, CreatedAt, Source)
            SELECT 
                'BKG-' + CAST(NEWID() AS VARCHAR(36)),
                @CustomerId,
                @RoomId,
                CASE WHEN N % 4 = 0 THEN 'Standard' WHEN N % 4 = 1 THEN 'Deluxe' WHEN N % 4 = 2 THEN 'Suite' ELSE 'Villa' END,
                DATEADD(DAY, (N % 365), GETUTCDATE()),
                DATEADD(DAY, (N % 365) + 2, GETUTCDATE()),
                1,
                CASE WHEN N % 4 = 0 THEN 'Pending' WHEN N % 4 = 1 THEN 'Confirmed' WHEN N % 4 = 2 THEN 'CheckedIn' ELSE 'CheckedOut' END,
                200,
                100,
                GETUTCDATE(),
                'Direct'
            FROM Tally;
            ";

            var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300; // 5 minutes max
            
            var sw = Stopwatch.StartNew();
            await cmd.ExecuteNonQueryAsync();
            sw.Stop();
            
            Console.WriteLine($"Data Generation Complete! Took {sw.ElapsedMilliseconds}ms");
        }

        static async Task RunLoadTestAsync()
        {
            Console.Write("Target API URL (e.g. http://localhost:5130/api/bookings): ");
            string targetUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(targetUrl)) targetUrl = "http://localhost:5130/api/bookings";

            Console.Write("Number of Requests (default 5000): ");
            int totalRequests = 5000;
            if (int.TryParse(Console.ReadLine(), out int reqs)) totalRequests = reqs;

            Console.Write("Concurrency (default 50): ");
            int concurrency = 50;
            if (int.TryParse(Console.ReadLine(), out int conc)) concurrency = conc;

            Console.WriteLine($"Starting Load Test -> URL: {targetUrl}, Total: {totalRequests}, Concurrency: {concurrency}");
            
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = concurrency * 2
            };
            
            using var httpClient = new HttpClient(handler);
            var sw = Stopwatch.StartNew();

            int successCount = 0;
            int failCount = 0;
            
            var options = new ParallelOptions { MaxDegreeOfParallelism = concurrency };
            
            await Parallel.ForEachAsync(Enumerable.Range(0, totalRequests), options, async (i, ct) =>
            {
                try
                {
                    var response = await httpClient.GetAsync(targetUrl, ct);
                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref failCount);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref failCount);
                }
            });

            sw.Stop();
            double seconds = sw.Elapsed.TotalSeconds;
            double rps = totalRequests / seconds;

            Console.WriteLine("=== LOAD TEST RESULTS ===");
            Console.WriteLine($"Total Time: {seconds:F2}s");
            Console.WriteLine($"Success: {successCount}");
            Console.WriteLine($"Failed: {failCount}");
            Console.WriteLine($"Requests/sec: {rps:F2}");
            Console.WriteLine($"Avg Latency: {(sw.ElapsedMilliseconds / (double)totalRequests):F2}ms");
        }
    }
}
