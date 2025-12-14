using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, int businessId)
    {
        // Validate role
        var validRoles = new[] { "Employee", "Manager", "Admin" };
        if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Invalid role: {request.Role}. Valid roles are: Employee, Manager, Admin");
        }

        // Check if phone already exists for this business
        var existingUser = await _context.Users
            .Where(u => u.Phone == request.Phone && u.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this phone number already exists in your business");
        }

        // Hash password
        var passwordHash = AuthService.HashPassword(request.Password);

        var user = new User
        {
            BusinessId = businessId,
            Name = request.Name,
            Phone = request.Phone,
            PasswordHash = passwordHash,
            Role = request.Role,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Employee created: UserId={UserId}, BusinessId={BusinessId}, Name={Name}, Role={Role}",
            user.Id, user.BusinessId, user.Name, user.Role);

        return MapToEmployeeResponse(user);
    }

    public async Task<List<EmployeeResponse>> GetAllEmployeesAsync(int businessId, bool? activeOnly = null)
    {
        var query = _context.Users
            .Where(u => u.BusinessId == businessId);

        if (activeOnly == true)
        {
            query = query.Where(u => u.IsActive);
        }

        var users = await query
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToEmployeeResponse).ToList();
    }

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(int employeeId, int businessId)
    {
        var user = await _context.Users
            .Where(u => u.Id == employeeId && u.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return user != null ? MapToEmployeeResponse(user) : null;
    }

    public async Task<EmployeeResponse?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request, int businessId)
    {
        var user = await _context.Users
            .Where(u => u.Id == employeeId && u.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return null;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            user.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            // Check if phone already exists for another user in this business
            var existingUser = await _context.Users
                .Where(u => u.Phone == request.Phone && u.BusinessId == businessId && u.Id != employeeId)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this phone number already exists in your business");
            }

            user.Phone = request.Phone;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
            {
                throw new InvalidOperationException("Password must be at least 6 characters");
            }

            user.PasswordHash = AuthService.HashPassword(request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var validRoles = new[] { "Employee", "Manager", "Admin" };
            if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Invalid role: {request.Role}. Valid roles are: Employee, Manager, Admin");
            }

            user.Role = request.Role;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Employee updated: UserId={UserId}, BusinessId={BusinessId}, Name={Name}, Role={Role}",
            user.Id, user.BusinessId, user.Name, user.Role);

        return MapToEmployeeResponse(user);
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId, int businessId)
    {
        var user = await _context.Users
            .Where(u => u.Id == employeeId && u.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return false;
        }

        // Check if user has created orders or payments (soft delete by setting IsActive = false)
        var hasOrders = await _context.Orders.AnyAsync(o => o.CreatedBy == employeeId);
        var hasPayments = await _context.Payments.AnyAsync(p => p.CreatedBy == employeeId);
        var hasAppointments = await _context.Appointments.AnyAsync(a => a.EmployeeId == employeeId);

        if (hasOrders || hasPayments || hasAppointments)
        {
            // Soft delete: deactivate instead of deleting
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Employee deactivated (soft delete): UserId={UserId}, BusinessId={BusinessId}, Name={Name}",
                user.Id, user.BusinessId, user.Name);
        }
        else
        {
            // Hard delete if no related records
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Employee deleted: UserId={UserId}, BusinessId={BusinessId}, Name={Name}",
                user.Id, user.BusinessId, user.Name);
        }

        return true;
    }

    private EmployeeResponse MapToEmployeeResponse(User user)
    {
        return new EmployeeResponse
        {
            Id = user.Id,
            BusinessId = user.BusinessId,
            Name = user.Name,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
