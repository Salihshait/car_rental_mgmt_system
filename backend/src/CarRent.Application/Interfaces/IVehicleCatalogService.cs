using CarRent.Application.DTOs.VehicleCatalog;

namespace CarRent.Application.Interfaces;

public interface IVehicleCatalogService
{
    Task<IEnumerable<VehicleCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<VehicleCategoryDto> CreateCategoryAsync(SaveVehicleCategoryRequest request, CancellationToken cancellationToken = default);
    Task<VehicleCategoryDto> UpdateCategoryAsync(Guid id, SaveVehicleCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(SaveBrandRequest request, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateBrandAsync(Guid id, SaveBrandRequest request, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ModelDto>> GetModelsAsync(CancellationToken cancellationToken = default);
    Task<ModelDto> CreateModelAsync(SaveModelRequest request, CancellationToken cancellationToken = default);
    Task<ModelDto> UpdateModelAsync(Guid id, SaveModelRequest request, CancellationToken cancellationToken = default);
    Task DeleteModelAsync(Guid id, CancellationToken cancellationToken = default);
}
