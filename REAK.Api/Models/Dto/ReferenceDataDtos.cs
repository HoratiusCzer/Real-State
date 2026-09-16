using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Dto;

public record CreatePropertyTypeRequest([Required, MaxLength(100)] string Name);
public record CreatePropertySubtypeRequest([Required] Guid PropertyTypeId, [Required, MaxLength(100)] string Name);
public record CreatePurposeRequest([Required, MaxLength(100)] string Name);
public record CreateAmenityRequest([Required, MaxLength(100)] string Name);
public record CreateDistrictRequest([Required] Guid ProvinceId, [Required, MaxLength(100)] string Name);
public record CreateMunicipalityRequest([Required] Guid DistrictId, [Required, MaxLength(150)] string Name);
public record CreateWardRequest([Required] Guid MunicipalityId, [Required] int Number);
public record CreateLocalityRequest([Required] Guid WardId, [Required, MaxLength(150)] string Name);
