using Microsoft.AspNetCore.Mvc;

namespace QuanLyResort.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    [Route("/")]
    public IActionResult Index()
    {
        Console.WriteLine("=== DEBUG: HomeController.Index() called ===");
        Console.WriteLine($"Request Path: {Request.Path}");
        Console.WriteLine($"Request Query: {Request.QueryString}");
        Console.WriteLine("Redirecting to /customer/index.html");
        
        return Redirect("/customer/index.html");
    }
}
