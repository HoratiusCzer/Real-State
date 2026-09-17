using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Dto;

public record FeatureFlagDto(Guid Id, string Key, bool IsEnabled, DateTime UpdatedAt);
public record SetFeatureFlagRequest([Required] bool IsEnabled);
