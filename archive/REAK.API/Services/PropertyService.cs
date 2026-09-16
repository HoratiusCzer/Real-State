using Microsoft.EntityFrameworkCore;
using REAK.API.Data;
using REAK.API.Middleware;
using REAK.API.Models.DTOs;
using REAK.API.Models.Entities;

namespace REAK.API.Services;

public class PropertyService : IPropertyService
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ReakDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(ReakDbContext context, IWebHostEnvironment environment, ILogger<PropertyService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PropertyResponse>> SearchAsync(PropertySearchRequest request)
    {
        var query = _context.Properties
            .Include(p => p.Branch)
            .Include(p => p.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                p.Location.ToLower().Contains(term));
        }

        if (request.Type.HasValue)
        {
            query = query.Where(p => p.Type == request.Type.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (request.MinArea.HasValue)
        {
            query = query.Where(p => p.Area >= request.MinArea.Value);
        }

        if (request.MaxArea.HasValue)
        {
            query = query.Where(p => p.Area <= request.MaxArea.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            var location = request.Location.Trim().ToLower();
            query = query.Where(p => p.Location.ToLower().Contains(location));
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(p => p.BranchId == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Bedrooms))
        {
            query = query.Where(p => p.Bedrooms == request.Bedrooms);
        }

        if (!string.IsNullOrWhiteSpace(request.Bathrooms))
        {
            query = query.Where(p => p.Bathrooms == request.Bathrooms);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        }

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var totalCount = await query.CountAsync();

        var properties = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var data = properties.Select(MapToResponse).ToList();

        return PaginatedResponse<PropertyResponse>.Create(data, request.Page, request.PageSize, totalCount);
    }

    private static IQueryable<Property> ApplySorting(IQueryable<Property> query, string? sortBy, bool descending)
    {
        Func<IQueryable<Property>, IOrderedQueryable<Property>> orderBy = sortBy?.ToLower() switch
        {
            "price" => descending ? q => q.OrderByDescending(p => p.Price) : q => q.OrderBy(p => p.Price),
            "area" => descending ? q => q.OrderByDescending(p => p.Area) : q => q.OrderBy(p => p.Area),
            "title" => descending ? q => q.OrderByDescending(p => p.Title) : q => q.OrderBy(p => p.Title),
            _ => descending ? q => q.OrderByDescending(p => p.CreatedAt) : q => q.OrderBy(p => p.CreatedAt)
        };

        return orderBy(query);
    }

    public async Task<PropertyResponse> GetByIdAsync(int id)
    {
        var property = await _context.Properties
            .Include(p => p.Branch)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            throw new NotFoundException("Property", id);
        }

        return MapToResponse(property);
    }

    public async Task<PropertyResponse> CreateAsync(CreatePropertyRequest request)
    {
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId);
        if (!branchExists)
        {
            throw new BadRequestException($"Branch with id '{request.BranchId}' does not exist");
        }

        var property = new Property
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Status = request.Status ?? Models.Enums.PropertyStatus.Available,
            Price = request.Price,
            Area = request.Area,
            Location = request.Location,
            Address = request.Address,
            BranchId = request.BranchId,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            YearBuilt = request.YearBuilt,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Property created: {PropertyId} - {Title}", property.Id, property.Title);

        return await GetByIdAsync(property.Id);
    }

    public async Task<PropertyResponse> UpdateAsync(int id, UpdatePropertyRequest request)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (property == null)
        {
            throw new NotFoundException("Property", id);
        }

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId);
        if (!branchExists)
        {
            throw new BadRequestException($"Branch with id '{request.BranchId}' does not exist");
        }

        property.Title = request.Title;
        property.Description = request.Description;
        property.Type = request.Type;
        property.Status = request.Status;
        property.Price = request.Price;
        property.Area = request.Area;
        property.Location = request.Location;
        property.Address = request.Address;
        property.BranchId = request.BranchId;
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;
        property.YearBuilt = request.YearBuilt;
        property.Latitude = request.Latitude;
        property.Longitude = request.Longitude;
        property.IsFeatured = request.IsFeatured;
        property.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Property updated: {PropertyId}", property.Id);

        return await GetByIdAsync(property.Id);
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            throw new NotFoundException("Property", id);
        }

        foreach (var image in property.Images)
        {
            DeleteImageFile(image.ImageUrl);
        }

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Property deleted: {PropertyId}", id);
    }

    public async Task<PropertyResponse> UpdateStatusAsync(int id, UpdatePropertyStatusRequest request)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (property == null)
        {
            throw new NotFoundException("Property", id);
        }

        property.Status = request.Status;
        property.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Property {PropertyId} status updated to {Status}", id, request.Status);

        return await GetByIdAsync(id);
    }

    public async Task<List<PropertyImageResponse>> UploadImagesAsync(int id, List<IFormFile> files)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            throw new NotFoundException("Property", id);
        }

        if (files.Count == 0)
        {
            throw new BadRequestException("At least one image file is required");
        }

        var uploadsRoot = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "properties", id.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var hasPrimary = property.Images.Any(i => i.IsPrimary);
        var nextOrder = property.Images.Count == 0 ? 0 : property.Images.Max(i => i.DisplayOrder) + 1;
        var addedImages = new List<PropertyImage>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
            {
                throw new BadRequestException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
            }

            if (file.Length > MaxImageSizeBytes)
            {
                throw new BadRequestException($"File '{file.FileName}' exceeds the maximum allowed size of 5 MB");
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var image = new PropertyImage
            {
                PropertyId = id,
                ImageUrl = $"/uploads/properties/{id}/{fileName}",
                IsPrimary = !hasPrimary && addedImages.Count == 0,
                DisplayOrder = nextOrder++,
                CreatedAt = DateTime.UtcNow
            };

            addedImages.Add(image);
        }

        _context.PropertyImages.AddRange(addedImages);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Uploaded {Count} image(s) for property {PropertyId}", addedImages.Count, id);

        return addedImages.Select(MapImageToResponse).ToList();
    }

    public async System.Threading.Tasks.Task DeleteImageAsync(int id, int imageId)
    {
        var image = await _context.PropertyImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.PropertyId == id);

        if (image == null)
        {
            throw new NotFoundException("Property image", imageId);
        }

        var wasPrimary = image.IsPrimary;

        DeleteImageFile(image.ImageUrl);
        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync();

        if (wasPrimary)
        {
            var nextImage = await _context.PropertyImages
                .Where(i => i.PropertyId == id)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync();

            if (nextImage != null)
            {
                nextImage.IsPrimary = true;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Deleted image {ImageId} from property {PropertyId}", imageId, id);
    }

    public async Task<PropertyImageResponse> SetPrimaryImageAsync(int id, int imageId)
    {
        var images = await _context.PropertyImages
            .Where(i => i.PropertyId == id)
            .ToListAsync();

        var target = images.FirstOrDefault(i => i.Id == imageId);
        if (target == null)
        {
            throw new NotFoundException("Property image", imageId);
        }

        foreach (var image in images)
        {
            image.IsPrimary = image.Id == imageId;
        }

        await _context.SaveChangesAsync();

        return MapImageToResponse(target);
    }

    private void DeleteImageFile(string imageUrl)
    {
        try
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image file for URL {ImageUrl}", imageUrl);
        }
    }

    private static PropertyResponse MapToResponse(Property property)
    {
        return new PropertyResponse
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            Type = property.Type,
            Status = property.Status,
            Price = property.Price,
            Area = property.Area,
            Location = property.Location,
            Address = property.Address,
            BranchId = property.BranchId,
            BranchName = property.Branch?.Name,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            YearBuilt = property.YearBuilt,
            Latitude = property.Latitude,
            Longitude = property.Longitude,
            IsFeatured = property.IsFeatured,
            CreatedAt = property.CreatedAt,
            UpdatedAt = property.UpdatedAt,
            Images = property.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .Select(MapImageToResponse)
                .ToList()
        };
    }

    private static PropertyImageResponse MapImageToResponse(PropertyImage image)
    {
        return new PropertyImageResponse
        {
            Id = image.Id,
            PropertyId = image.PropertyId,
            ImageUrl = image.ImageUrl,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder,
            Caption = image.Caption,
            CreatedAt = image.CreatedAt
        };
    }
}
