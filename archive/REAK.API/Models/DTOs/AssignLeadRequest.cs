using System.ComponentModel.DataAnnotations;

namespace REAK.API.Models.DTOs;

public class AssignLeadRequest
{
    [Required]
    public int AgentId { get; set; }
}
