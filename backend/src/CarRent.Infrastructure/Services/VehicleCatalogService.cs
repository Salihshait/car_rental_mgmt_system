using CarRent.Application.DTOs.VehicleCatalog;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleCatalogService : IVehicleCatalogService
{
    private readonly CarRentDbContext _context;

    public VehicleCatalogService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VehicleCategories
            .AsNoTracking()
            .Select(c => new VehicleCategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleCategoryDto> CreateCategoryAsync(SaveVehicleCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.VehicleCategories.AnyAsync(c => c.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException("A category with this name already exists.");
        }

        var category = new VehicleCategory { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };
        await _context.VehicleCategories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new VehicleCategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task<VehicleCategoryDto> UpdateCategoryAsync(Guid id, SaveVehicleCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _context.VehicleCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        category.Name = request.Name;
        category.Description = request.Description;
        await _context.SaveChangesAsync(cancellationToken);
        return new VehicleCategoryDto { Id = category.Id, Name = category.Name, Description = category.Description };
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.VehicleCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        if (await _context.Models.AnyAsync(m => m.CategoryId == id, cancellationToken))
        {
            throw new InvalidOperationException("This category has models assigned and cannot be deleted.");
        }

        _context.VehicleCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .AsNoTracking()
            .Select(b => new BrandDto { Id = b.Id, Name = b.Name })
            .ToListAsync(cancellationToken);
    }

    public async Task<BrandDto> CreateBrandAsync(SaveBrandRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Brands.AnyAsync(b => b.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException("A brand with this name already exists.");
        }

        var brand = new Brand { Id = Guid.NewGuid(), Name = request.Name };
        await _context.Brands.AddAsync(brand, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new BrandDto { Id = brand.Id, Name = brand.Name };
    }

    public async Task<BrandDto> UpdateBrandAsync(Guid id, SaveBrandRequest request, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Brand not found.");

        brand.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return new BrandDto { Id = brand.Id, Name = brand.Name };
    }

    public async Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Brand not found.");

        if (await _context.Models.AnyAsync(m => m.BrandId == id, cancellationToken))
        {
            throw new InvalidOperationException("This brand has models assigned and cannot be deleted.");
        }

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ModelDto>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Models
            .AsNoTracking()
            .Include(m => m.Brand)
            .Include(m => m.Category)
            .Select(m => new ModelDto
            {
                Id = m.Id,
                Name = m.Name,
                BrandId = m.BrandId,
                BrandName = m.Brand.Name,
                CategoryId = m.CategoryId,
                CategoryName = m.Category.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ModelDto> CreateModelAsync(SaveModelRequest request, CancellationToken cancellationToken = default)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == request.BrandId, cancellationToken)
            ?? throw new InvalidOperationException("The selected brand does not exist.");
        var category = await _context.VehicleCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException("The selected category does not exist.");

        var model = new Model { Id = Guid.NewGuid(), Name = request.Name, BrandId = brand.Id, CategoryId = category.Id };
        await _context.Models.AddAsync(model, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ModelDto { Id = model.Id, Name = model.Name, BrandId = brand.Id, BrandName = brand.Name, CategoryId = category.Id, CategoryName = category.Name };
    }

    public async Task<ModelDto> UpdateModelAsync(Guid id, SaveModelRequest request, CancellationToken cancellationToken = default)
    {
        var model = await _context.Models.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Model not found.");
        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == request.BrandId, cancellationToken)
            ?? throw new InvalidOperationException("The selected brand does not exist.");
        var category = await _context.VehicleCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException("The selected category does not exist.");

        model.Name = request.Name;
        model.BrandId = brand.Id;
        model.CategoryId = category.Id;
        await _context.SaveChangesAsync(cancellationToken);
        return new ModelDto { Id = model.Id, Name = model.Name, BrandId = brand.Id, BrandName = brand.Name, CategoryId = category.Id, CategoryName = category.Name };
    }

    public async Task DeleteModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Models.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Model not found.");

        if (await _context.Vehicles.AnyAsync(v => v.ModelId == id, cancellationToken))
        {
            throw new InvalidOperationException("This model is assigned to one or more vehicles and cannot be deleted.");
        }

        _context.Models.Remove(model);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
