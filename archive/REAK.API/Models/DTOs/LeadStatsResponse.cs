namespace REAK.API.Models.DTOs;

public class LeadStatsResponse
{
    public int TotalLeads { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> BySource { get; set; } = new();
    public int WonCount { get; set; }
    public int LostCount { get; set; }
    public double ConversionRate { get; set; }
    public int UnassignedCount { get; set; }
    public int OverdueFollowUpCount { get; set; }
}
