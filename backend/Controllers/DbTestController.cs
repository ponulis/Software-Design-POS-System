using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/db")]
public class DbTestController : ControllerBase
{
    private readonly AppDbContext _db;

    public DbTestController(AppDbContext db)
    {
        _db = db;
    }

    // Test 1 — check DB connection by reading any table
    [HttpGet("businesses")]
    public IActionResult GetBusinesses()
    {
        try
        {
            var result = _db.Businesses.ToList();
            return Ok(new 
            {
                message = "Database connection successful!",
                count = result.Count,
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            {
                message = "Database connection FAILED",
                error = ex.Message
            });
        }
    }

    // Test 2 — insert a test business
    [HttpPost("add-business")]
    public IActionResult AddBusiness()
    {
        var business = new Business 
        {
            Name = "Test Business",
            Address = "123 Test Street",
            PhoneNumber = "123456789"
        };

        _db.Businesses.Add(business);
        _db.SaveChanges();

        return Ok(new
        {
            message = "Business inserted!",
            id = business.Id
        });
    }
}