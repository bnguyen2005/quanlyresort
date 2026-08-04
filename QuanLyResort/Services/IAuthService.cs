using QuanLyResort.Models;

namespace QuanLyResort.Services;

public interface IAuthService
{
    Task<(bool Success, string? Token, User? User)> LoginAsync(string emailOrUsername, string password, string? role = null);
    Task<(bool Success, string? Token, User? User)> CustomerLoginAsync(string emailOrUsername, string password);
    Task<User?> RegisterCustomerAsync(Customer customer, string username, string password);
    Task<User?> RegisterCustomerAsync(RegisterRequest request);
    string GenerateJwtToken(User user);
}

