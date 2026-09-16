using REAK.API.Models.DTOs;

namespace REAK.API.Services;

public interface ILeadService
{
    Task<PaginatedResponse<LeadResponse>> SearchAsync(LeadSearchRequest request);
    Task<LeadResponse> GetByIdAsync(int id);
    Task<LeadResponse> CreateAsync(CreateLeadRequest request);
    Task<LeadResponse> UpdateAsync(int id, UpdateLeadRequest request);
    Task<LeadResponse> UpdateStatusAsync(int id, UpdateLeadStatusRequest request);
    Task<LeadResponse> AssignAsync(int id, AssignLeadRequest request);
    Task<LeadResponse> UnassignAsync(int id);
    Task<LeadResponse> ScheduleFollowUpAsync(int id, UpdateLeadFollowUpRequest request);
    Task DeleteAsync(int id);
    Task<LeadStatsResponse> GetStatsAsync();
}
