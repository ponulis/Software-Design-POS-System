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
public class TaxController : ControllerBase
{
    private readonly AppDbContext _db;

    public TaxController(AppDbContext db)
    {
        _db = db;
    }

    // CREATE
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create(TaxCreateDto dto)
    {
        if (!await _db.Businesses.AnyAsync(b => b.Id == dto.BusinessId))
            return BadRequest("Business does not exist.");

        if (dto.EffectiveFrom >= dto.EffectiveTo)
            return BadRequest("Invalid date range.");

        var tax = new Tax
        {
            BusinessId = dto.BusinessId,
            Name = dto.Name,
            Rate = dto.Rate,
            AppliesTo = dto.AppliesTo,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo
        };

        _db.Tax.Add(tax);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Tax created", tax.Id });
    }

    // READ ALL
    [HttpGet]
    public async Task<ActionResult<List<TaxReadDto>>> GetAll()
    {
        var taxes = await _db.Tax
            .Select(t => new TaxReadDto
            {
                Id = t.Id,
                BusinessId = t.BusinessId,
                Name = t.Name,
                Rate = t.Rate,
                AppliesTo = t.AppliesTo,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo
            })
            .ToListAsync();

        return Ok(taxes);
    }

    // READ ONE
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var t = await _db.Tax.FindAsync(id);
        if (t == null) return NotFound("Tax not found.");

        return Ok(new TaxReadDto
        {
            Id = t.Id,
            BusinessId = t.BusinessId,
            Name = t.Name,
            Rate = t.Rate,
            AppliesTo = t.AppliesTo,
            EffectiveFrom = t.EffectiveFrom,
            EffectiveTo = t.EffectiveTo
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Update(int id, TaxUpdateDto dto)
    {
        var tax = await _db.Tax.FindAsync(id);
        if (tax == null) return NotFound("Tax not found.");

        if (dto.Name != null) tax.Name = dto.Name;
        if (dto.Rate != null) tax.Rate = dto.Rate.Value;
        if (dto.AppliesTo != null) tax.AppliesTo = dto.AppliesTo;
        if (dto.EffectiveFrom != null) tax.EffectiveFrom = dto.EffectiveFrom.Value;
        if (dto.EffectiveTo != null) tax.EffectiveTo = dto.EffectiveTo.Value;

        if (tax.EffectiveFrom >= tax.EffectiveTo)
            return BadRequest("Invalid date range.");

        await _db.SaveChangesAsync();
        return Ok("Tax updated.");
    }

    // DELETE
    [HttpDelete("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete(int id)
    {
        var tax = await _db.Tax.FindAsync(id);
        if (tax == null) return NotFound("Tax not found.");

        _db.Tax.Remove(tax);
        await _db.SaveChangesAsync();

        return Ok("Tax deleted.");
    }
}
