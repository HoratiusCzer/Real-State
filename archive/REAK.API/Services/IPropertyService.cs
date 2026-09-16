using REAK.API.Models.DTOs;

namespace REAK.API.Services;

public interface IPropertyService
{
    Task<PaginatedResponse<PropertyResponse>> SearchAsync(PropertySearchRequest request);
    Task<PropertyResponse> GetByIdAsync(int id);
    Task<PropertyResponse> CreateAsync(CreatePropertyRequest request);
    Task<PropertyResponse> UpdateAsync(int id, UpdatePropertyRequest request);
    Task DeleteAsync(int id);
    Task<PropertyResponse> UpdateStatusAsync(int id, UpdatePropertyStatusRequest request);
    Task<List<PropertyImageResponse>> UploadImagesAsync(int id, List<IFormFile> files);
    Task DeleteImageAsync(int id, int imageId);
    Task<PropertyImageResponse> SetPrimaryImageAsync(int id, int imageId);
}
