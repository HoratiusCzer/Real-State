using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class LeadResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientPhone { get; set; }
    public int? PropertyId { get; set; }
    public string? PropertyTitle { get; set; }
    public LeadStatus Status { get; set; }
    public LeadSource Source { get; set; }
    public int? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? Notes { get; set; }
    public decimal? Budget { get; set; }
    public string? Requirements { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
