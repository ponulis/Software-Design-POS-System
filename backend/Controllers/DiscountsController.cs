using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscountsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DiscountsController(AppDbContext db)
    {
        _db = db;
    }

    // CREATE
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create(DiscountCreateDto dto)
    {
        if (!await _db.Businesses.AnyAsync(b => b.Id == dto.BusinessId))
            return BadRequest("Business not found.");

        if (dto.ValidFrom >= dto.ValidTo)
            return BadRequest("Invalid date range.");

        if (dto.AmountType != "Fixed" && dto.AmountType != "Percentage")
            return BadRequest("AmountType must be Fixed or Percentage.");

        var discount = new Discount
        {
            Business = await _db.Businesses.FindAsync(dto.BusinessId),
            Name = dto.Name,
            Description = dto.Description,
            Amount = dto.Amount,
            AmountType = dto.AmountType,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo
        };

        _db.Discounts.Add(discount);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Discount created", discount.Id });
    }

    // READ ALL
    [HttpGet]
    public async Task<ActionResult<List<DiscountReadDto>>> GetAll()
    {
        var discounts = await _db.Discounts
            .Select(d => new DiscountReadDto
            {
                Id = d.Id,
                BusinessId = d.Business.Id,
                Name = d.Name,
                Description = d.Description,
                Amount = d.Amount,
                AmountType = d.AmountType,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo
            })
            .ToListAsync();

        return Ok(discounts);
    }

    // READ ONE
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var d = await _db.Discounts.Include(x => x.Business).FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound("Discount not found.");

        return Ok(new DiscountReadDto
        {
            Id = d.Id,
            BusinessId = d.Business.Id,
            Name = d.Name,
            Description = d.Description,
            Amount = d.Amount,
            AmountType = d.AmountType,
            ValidFrom = d.ValidFrom,
            ValidTo = d.ValidTo
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Update(int id, DiscountUpdateDto dto)
    {
        var d = await _db.Discounts.Include(x => x.Business).FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound("Discount not found.");

        if (dto.Name != null) d.Name = dto.Name;
        if (dto.Description != null) d.Description = dto.Description;
        if (dto.Amount != null) d.Amount = dto.Amount.Value;
        if (dto.AmountType != null) d.AmountType = dto.AmountType;
        if (dto.ValidFrom != null) d.ValidFrom = dto.ValidFrom.Value;
        if (dto.ValidTo != null) d.ValidTo = dto.ValidTo.Value;

        if (d.ValidFrom >= d.ValidTo)
            return BadRequest("Invalid date range.");

        await _db.SaveChangesAsync();
        return Ok("Discount updated.");
    }

    // DELETE
    [HttpDelete("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Discounts.FindAsync(id);
        if (d == null) return NotFound("Discount not found.");

        // Prevent deletion if related orders exist
        if (await _db.Orders.AnyAsync(o => o.Discounts.Contains(d)))
            return BadRequest("Cannot delete discount used in orders.");

        _db.Discounts.Remove(d);
        await _db.SaveChangesAsync();

        return Ok("Discount deleted.");
    }
}
