using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly PasswordService _password;

        public AuthController(AppDbContext db, JwtService jwt, PasswordService password)
        {
            _db = db;
            _jwt = jwt;
            _password = password;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterEmployeeDto dto)
        {
            if (await _db.Employees.AnyAsync(e => e.Email == dto.Email))
                return BadRequest("An employee with this email already exists.");

            var employee = new Employee
            {
                BusinessId = dto.BusinessId,
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = _password.HashPassword(dto.Password)
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            return Ok("Employee registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginEmployeeDto dto)
        {
            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Email == dto.Email);

            if (employee == null || !_password.VerifyPassword(dto.Password, employee.PasswordHash))
                return Unauthorized("Invalid email or password.");

            var token = _jwt.GenerateToken(employee);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = employee.Email,
                Role = employee.Role,
                Name = employee.Name,
                BusinessId = employee.BusinessId
            });
        }
    }
}
