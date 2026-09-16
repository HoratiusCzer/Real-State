namespace REAK.API.Models.DTOs;

public class ClientSummaryResponse
{
    public ClientResponse Client { get; set; } = null!;
    public int TotalLeads { get; set; }
    public Dictionary<string, int> LeadsByStatus { get; set; } = new();
    public int TotalDeals { get; set; }
    public List<LeadResponse> RecentLeads { get; set; } = new();
}
