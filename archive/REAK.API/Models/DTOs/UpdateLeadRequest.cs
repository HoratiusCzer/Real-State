using System.ComponentModel.DataAnnotations;

namespace REAK.API.Models.DTOs;

public class UpdateLeadRequest
{
    public int? PropertyId { get; set; }

    public DateTime? FollowUpDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Budget { get; set; }

    [MaxLength(500)]
    public string? Requirements { get; set; }

    [Range(1, 5)]
    public int Priority { get; set; } = 1;
}
