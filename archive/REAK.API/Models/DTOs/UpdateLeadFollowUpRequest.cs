using System.ComponentModel.DataAnnotations;

namespace REAK.API.Models.DTOs;

public class UpdateLeadFollowUpRequest
{
    [Required]
    public DateTime FollowUpDate { get; set; }
}
