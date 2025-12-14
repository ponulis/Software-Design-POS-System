using backend.DTOs;
using backend.Extensions;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(UserService userService, ILogger<EmployeesController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="request">Employee details including name, password, phone, and role</param>
    /// <returns>Created employee information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/employees
    ///     {
    ///         "name": "John Doe",
    ///         "password": "password123",
    ///         "phone": "+1234567890",
    ///         "role": "Employee"
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var employee = await _userService.CreateEmployeeAsync(request, businessId);
            return CreatedAtAction(nameof(GetEmployeeById), new { employeeId = employee.Id }, employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new { message = "An error occurred while creating the employee" });
        }
    }

    /// <summary>
    /// Get all employees for the authenticated user's business
    /// </summary>
    /// <param name="activeOnly">Filter to show only active employees</param>
    /// <returns>List of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmployees([FromQuery] bool? activeOnly = null)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var employees = await _userService.GetAllEmployeesAsync(businessId, activeOnly);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new { message = "An error occurred while retrieving employees" });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeById(int employeeId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var employee = await _userService.GetEmployeeByIdAsync(employeeId, businessId);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee");
            return StatusCode(500, new { message = "An error occurred while retrieving the employee" });
        }
    }

    /// <summary>
    /// Modify employee details (PATCH per API contract)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="request">Employee fields to modify</param>
    /// <returns>Updated employee information</returns>
    [HttpPatch("{employeeId}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] UpdateEmployeeRequest request)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var employee = await _userService.UpdateEmployeeAsync(employeeId, request, businessId);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(employee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee");
            return StatusCode(500, new { message = "An error occurred while updating the employee" });
        }
    }

    /// <summary>
    /// Delete employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{employeeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int employeeId)
    {
        try
        {
            var businessIdNullable = User.GetBusinessId();
            if (!businessIdNullable.HasValue)
            {
                throw new UnauthorizedAccessException("Business ID not found in token");
            }

            var businessId = businessIdNullable.Value;
            var deleted = await _userService.DeleteEmployeeAsync(employeeId, businessId);

            if (!deleted)
            {
                return NotFound(new { message = "Employee not found" });
            }

            return Ok(new { message = "Employee deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee");
            return StatusCode(500, new { message = "An error occurred while deleting the employee" });
        }
    }
}
