using System.ComponentModel.DataAnnotations;
using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class UpdateLeadStatusRequest
{
    [Required]
    public LeadStatus Status { get; set; }
}
