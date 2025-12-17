using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Npgsql;

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

        // Try to include modifications and inventory, but handle gracefully if tables don't exist
        try
        {
            var products = await query
                .Include(p => p.ModificationAssignments)
                    .ThenInclude(ma => ma.Modification)
                        .ThenInclude(m => m.Values)
                .Include(p => p.InventoryItems)
                    .ThenInclude(ii => ii.ModificationValues)
                        .ThenInclude(imv => imv.ModificationValue)
                            .ThenInclude(mv => mv.Modification)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return products.Select(MapToProductResponse).ToList();
        }
        catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
        {
            // Tables don't exist yet - return products without modifications/inventory
            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return products.Select(MapToProductResponse).ToList();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("does not exist") == true || 
                                             dbEx.InnerException?.Message.Contains("relation") == true)
        {
            // Tables don't exist yet - return products without modifications/inventory
            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return products.Select(MapToProductResponse).ToList();
        }
    }

    public async Task<ProductResponse?> GetProductByIdAsync(int productId, int businessId)
    {
        try
        {
            var product = await _context.Products
                .Where(p => p.Id == productId && p.BusinessId == businessId)
                .Include(p => p.ModificationAssignments)
                    .ThenInclude(ma => ma.Modification)
                        .ThenInclude(m => m.Values)
                .Include(p => p.InventoryItems)
                    .ThenInclude(ii => ii.ModificationValues)
                        .ThenInclude(imv => imv.ModificationValue)
                            .ThenInclude(mv => mv.Modification)
                .FirstOrDefaultAsync();

            return product != null ? MapToProductResponse(product) : null;
        }
        catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
        {
            // Tables don't exist yet - return product without modifications/inventory
            var product = await _context.Products
                .Where(p => p.Id == productId && p.BusinessId == businessId)
                .FirstOrDefaultAsync();

            return product != null ? MapToProductResponse(product) : null;
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("does not exist") == true || 
                                             dbEx.InnerException?.Message.Contains("relation") == true)
        {
            // Tables don't exist yet - return product without modifications/inventory
            var product = await _context.Products
                .Where(p => p.Id == productId && p.BusinessId == businessId)
                .FirstOrDefaultAsync();

            return product != null ? MapToProductResponse(product) : null;
        }
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

        // Assign modifications to product
        if (request.ModificationIds != null && request.ModificationIds.Any())
        {
            // Validate that all modifications belong to this business
            var modifications = await _context.ProductModifications
                .Where(m => request.ModificationIds.Contains(m.Id) && m.BusinessId == businessId)
                .ToListAsync();

            if (modifications.Count != request.ModificationIds.Count)
            {
                throw new InvalidOperationException("One or more modifications not found or don't belong to your business");
            }

            foreach (var modification in modifications)
            {
                var assignment = new ProductModificationAssignment
                {
                    ProductId = product.Id,
                    ModificationId = modification.Id
                };
                product.ModificationAssignments.Add(assignment);
            }
        }

        // Create inventory items
        if (request.InventoryItems != null && request.InventoryItems.Any())
        {
            foreach (var invRequest in request.InventoryItems)
            {
                // Skip inventory items with no modification values (for products without modifications)
                if (invRequest.ModificationValueIds == null || !invRequest.ModificationValueIds.Any())
                {
                    // If modifications are assigned but no values provided, skip this inventory item
                    if (request.ModificationIds != null && request.ModificationIds.Any())
                    {
                        continue; // Skip inventory items without modification values when modifications are assigned
                    }
                    
                    // Create a simple inventory item without modifications
                    var inventoryItem = new InventoryItem
                    {
                        ProductId = product.Id,
                        Quantity = invRequest.Quantity,
                        ModificationValuesJson = "{}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.InventoryItems.Add(inventoryItem);
                    continue;
                }

                // Build modification values dictionary
                var modificationValuesDict = new Dictionary<string, string>();
                var modificationValueIds = new List<int>();

                foreach (var kvp in invRequest.ModificationValueIds)
                {
                    if (kvp.Value <= 0)
                    {
                        continue; // Skip invalid IDs
                    }

                    var modificationValue = await _context.ProductModificationValues
                        .Include(mv => mv.Modification)
                        .Where(mv => mv.Id == kvp.Value && mv.Modification.BusinessId == businessId)
                        .FirstOrDefaultAsync();

                    if (modificationValue == null)
                    {
                        throw new InvalidOperationException($"Modification value with ID {kvp.Value} not found");
                    }

                    modificationValuesDict[modificationValue.Modification.Name] = modificationValue.Value;
                    modificationValueIds.Add(modificationValue.Id);
                }

                // Only create inventory item if we have valid modification values
                if (modificationValueIds.Any())
                {
                    var inventoryItem = new InventoryItem
                    {
                        ProductId = product.Id,
                        Quantity = invRequest.Quantity,
                        ModificationValuesJson = JsonSerializer.Serialize(modificationValuesDict),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryItems.Add(inventoryItem);
                    await _context.SaveChangesAsync(); // Save to get the ID

                    // Link modification values after inventory item is saved
                    foreach (var valueId in modificationValueIds)
                    {
                        var inventoryModValue = new InventoryModificationValue
                        {
                            InventoryItemId = inventoryItem.Id,
                            ModificationValueId = valueId
                        };
                        _context.InventoryModificationValues.Add(inventoryModValue);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        // Reload with all relationships (handle gracefully if tables don't exist)
        try
        {
            await _context.Entry(product)
                .Collection(p => p.ModificationAssignments)
                .Query()
                .Include(ma => ma.Modification)
                    .ThenInclude(m => m.Values)
                .LoadAsync();

            await _context.Entry(product)
                .Collection(p => p.InventoryItems)
                .Query()
                .Include(ii => ii.ModificationValues)
                    .ThenInclude(imv => imv.ModificationValue)
                        .ThenInclude(mv => mv.Modification)
                .LoadAsync();
        }
        catch (PostgresException pgEx) when (pgEx.SqlState == "42P01") // Table does not exist
        {
            // Tables don't exist yet - product created but without modifications/inventory
            // This is okay, the product was still created successfully
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("does not exist") == true || 
                                             dbEx.InnerException?.Message.Contains("relation") == true)
        {
            // Tables don't exist yet - product created but without modifications/inventory
            // This is okay, the product was still created successfully
        }

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
        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Tags = product.Tags ?? new List<string>(),
            Available = product.Available,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Modifications = new List<ProductModificationResponse>(),
            InventoryItems = new List<InventoryItemResponse>()
        };

        // Map modifications
        if (product.ModificationAssignments != null && product.ModificationAssignments.Any())
        {
            response.Modifications = product.ModificationAssignments
                .Select(ma => ma.Modification)
                .Select(m => new ProductModificationResponse
                {
                    Id = m.Id,
                    Name = m.Name,
                    Values = m.Values?.Select(v => new ProductModificationValueResponse
                    {
                        Id = v.Id,
                        Value = v.Value,
                        CreatedAt = v.CreatedAt
                    }).ToList() ?? new List<ProductModificationValueResponse>(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToList();
        }

        // Map inventory items
        if (product.InventoryItems != null && product.InventoryItems.Any())
        {
            response.InventoryItems = product.InventoryItems.Select(ii =>
            {
                var modificationValuesDict = new Dictionary<string, string>();
                
                if (ii.ModificationValues != null && ii.ModificationValues.Any())
                {
                    foreach (var imv in ii.ModificationValues)
                    {
                        var modName = imv.ModificationValue.Modification.Name;
                        var modValue = imv.ModificationValue.Value;
                        modificationValuesDict[modName] = modValue;
                    }
                }
                else if (!string.IsNullOrEmpty(ii.ModificationValuesJson))
                {
                    try
                    {
                        modificationValuesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(ii.ModificationValuesJson) ?? new Dictionary<string, string>();
                    }
                    catch
                    {
                        // If JSON parsing fails, use empty dict
                    }
                }

                return new InventoryItemResponse
                {
                    Id = ii.Id,
                    ProductId = ii.ProductId,
                    Quantity = ii.Quantity,
                    ModificationValues = modificationValuesDict,
                    CreatedAt = ii.CreatedAt,
                    UpdatedAt = ii.UpdatedAt
                };
            }).ToList();
        }

        return response;
    }
}
