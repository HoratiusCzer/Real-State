using System.ComponentModel.DataAnnotations;
using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class UpdatePropertyStatusRequest
{
    [Required]
    public PropertyStatus Status { get; set; }
}
