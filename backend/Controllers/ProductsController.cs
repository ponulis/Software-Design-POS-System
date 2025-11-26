namespace backend.Controllers;

using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // CREATE
    [HttpPost]
    [Authorize(Roles = "User")] // For now
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        // Check business exists
        if (!await _db.Businesses.AnyAsync(b => b.Id == dto.BusinessId))
            return BadRequest("Business does not exist.");

        // Check tax exists
        var tax = await _db.Tax.FindAsync(dto.TaxId);
        if (tax == null)
            return BadRequest("Tax not found.");

        var product = new Product
        {
            BusinessId = dto.BusinessId,
            TaxId = dto.TaxId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            TaxRate = tax.Rate,
            Quantity = dto.Quantity
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Product created.", product.Id });
    }

    // READ (all)
    [HttpGet]
    public async Task<ActionResult<List<ProductReadDto>>> GetAll()
    {
        var products = await _db.Products
            .Select(p => new ProductReadDto
            {
                Id = p.Id,
                BusinessId = p.BusinessId,
                TaxId = p.TaxId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                TaxRate = p.TaxRate
            })
            .ToListAsync();

        return Ok(products);
    }

    // READ (one); could change it to read just the stock for one to validate orders
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound("Product not found.");

        return Ok(new ProductReadDto
        {
            Id = p.Id,
            BusinessId = p.BusinessId,
            TaxId = p.TaxId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            TaxRate = p.TaxRate,
            Quantity = p.Quantity
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound("Product not found.");

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price != null) product.Price = dto.Price.Value;

        if (dto.TaxId != null)
        {
            var tax = await _db.Tax.FindAsync(dto.TaxId);
            if (tax == null)
                return BadRequest("Tax does not exist.");

            product.TaxId = dto.TaxId.Value;
            product.TaxRate = tax.Rate;
        }

        await _db.SaveChangesAsync();
        return Ok("Product updated.");
    }

    // DELETE
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound("Product not found.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return Ok("Product deleted.");
    }
}
