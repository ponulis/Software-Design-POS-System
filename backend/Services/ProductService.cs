using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponse>> GetAllProductsAsync(int businessId, bool? availableOnly = null)
    {
        var query = _context.Products
            .Where(p => p.BusinessId == businessId);

        if (availableOnly == true)
        {
            query = query.Where(p => p.Available);
        }

        var products = await query
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToProductResponse).ToList();
    }

    public async Task<ProductResponse?> GetProductByIdAsync(int productId, int businessId)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && p.BusinessId == businessId)
            .FirstOrDefaultAsync();

        return product != null ? MapToProductResponse(product) : null;
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, int businessId)
    {
        var product = new Product
        {
            BusinessId = businessId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Tags = request.Tags ?? new List<string>(),
            Available = request.Available,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToProductResponse(product);
    }

    public async Task<ProductResponse?> UpdateProductAsync(int productId, UpdateProductRequest request, int businessId)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && p.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            product.Name = request.Name;
        }

        if (request.Description != null)
        {
            product.Description = request.Description;
        }

        if (request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        if (request.Tags != null)
        {
            product.Tags = request.Tags;
        }

        if (request.Available.HasValue)
        {
            product.Available = request.Available.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProductResponse(product);
    }

    public async Task<bool> DeleteProductAsync(int productId, int businessId)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId && p.BusinessId == businessId)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return false;
        }

        // Check if product is used in any orders
        var hasOrderItems = await _context.OrderItems
            .AnyAsync(oi => oi.MenuId == productId);

        if (hasOrderItems)
        {
            throw new InvalidOperationException("Cannot delete product that is used in orders. Set Available to false instead.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }

    private ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Tags = product.Tags ?? new List<string>(),
            Available = product.Available,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
