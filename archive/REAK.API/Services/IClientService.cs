using REAK.API.Models.DTOs;

namespace REAK.API.Services;

public interface IClientService
{
    Task<PaginatedResponse<ClientResponse>> SearchAsync(ClientSearchRequest request);
    Task<ClientResponse> GetByIdAsync(int id);
    Task<ClientSummaryResponse> GetSummaryAsync(int id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request);
    Task<ClientResponse> UpdateAsync(int id, UpdateClientRequest request);
    Task DeleteAsync(int id);
}
