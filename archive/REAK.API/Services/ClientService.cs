using Microsoft.EntityFrameworkCore;
using REAK.API.Data;
using REAK.API.Middleware;
using REAK.API.Models.DTOs;
using REAK.API.Models.Entities;
using REAK.API.Models.Enums;

namespace REAK.API.Services;

public class ClientService : IClientService
{
    private readonly ReakDbContext _context;
    private readonly ILogger<ClientService> _logger;

    public ClientService(ReakDbContext context, ILogger<ClientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ClientResponse>> SearchAsync(ClientSearchRequest request)
    {
        var query = _context.Clients
            .Include(c => c.Leads)
            .Include(c => c.Deals)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.Phone.Contains(term));
        }

        if (request.Source.HasValue)
        {
            query = query.Where(c => c.Source == request.Source.Value);
        }

        if (request.MinLeadScore.HasValue)
        {
            query = query.Where(c => c.LeadScore >= request.MinLeadScore.Value);
        }

        if (request.MaxLeadScore.HasValue)
        {
            query = query.Where(c => c.LeadScore <= request.MaxLeadScore.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "leadscore" => request.SortDescending ? query.OrderByDescending(c => c.LeadScore) : query.OrderBy(c => c.LeadScore),
            _ => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var clients = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var data = clients.Select(MapToResponse).ToList();

        return PaginatedResponse<ClientResponse>.Create(data, request.Page, request.PageSize, totalCount);
    }

    public async Task<ClientResponse> GetByIdAsync(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Leads)
            .Include(c => c.Deals)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            throw new NotFoundException("Client", id);
        }

        return MapToResponse(client);
    }

    public async Task<ClientSummaryResponse> GetSummaryAsync(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Deals)
            .Include(c => c.Leads).ThenInclude(l => l.Property)
            .Include(c => c.Leads).ThenInclude(l => l.AssignedAgent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            throw new NotFoundException("Client", id);
        }

        var leadsByStatus = client.Leads
            .GroupBy(l => l.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var recentLeads = client.Leads
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(LeadService.MapToResponse)
            .ToList();

        return new ClientSummaryResponse
        {
            Client = MapToResponse(client),
            TotalLeads = client.Leads.Count,
            LeadsByStatus = leadsByStatus,
            TotalDeals = client.Deals.Count,
            RecentLeads = recentLeads
        };
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request)
    {
        var normalizedEmail = request.Email.ToLower();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        User user;
        if (existingUser != null)
        {
            if (existingUser.Role != UserRole.Client)
            {
                throw new ConflictException($"A user with email '{request.Email}' already exists with role '{existingUser.Role}'");
            }

            var alreadyHasProfile = await _context.Clients.AnyAsync(c => c.UserId == existingUser.Id);
            if (alreadyHasProfile)
            {
                throw new ConflictException($"A client profile already exists for email '{request.Email}'");
            }

            user = existingUser;
        }
        else
        {
            user = new User
            {
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.Client,
                FullName = request.Name,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var client = new Client
        {
            UserId = user.Id,
            Name = request.Name,
            Phone = request.Phone,
            Email = normalizedEmail,
            Address = request.Address,
            Source = request.Source,
            LeadScore = request.LeadScore,
            Notes = request.Notes,
            CompanyName = request.CompanyName,
            Preferences = request.Preferences,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client created: {ClientId} - {Name}", client.Id, client.Name);

        return await GetByIdAsync(client.Id);
    }

    public async Task<ClientResponse> UpdateAsync(int id, UpdateClientRequest request)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (client == null)
        {
            throw new NotFoundException("Client", id);
        }

        client.Name = request.Name;
        client.Phone = request.Phone;
        client.Address = request.Address;
        client.Source = request.Source;
        client.LeadScore = request.LeadScore;
        client.Notes = request.Notes;
        client.CompanyName = request.CompanyName;
        client.Preferences = request.Preferences;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Client updated: {ClientId}", client.Id);

        return await GetByIdAsync(client.Id);
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (client == null)
        {
            throw new NotFoundException("Client", id);
        }

        var hasDeals = await _context.Deals.AnyAsync(d => d.ClientId == id);
        if (hasDeals)
        {
            throw new BadRequestException("Cannot delete a client with existing deals");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client deleted: {ClientId}", id);
    }

    private static ClientResponse MapToResponse(Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            UserId = client.UserId,
            Name = client.Name,
            Phone = client.Phone,
            Email = client.Email,
            Address = client.Address,
            Source = client.Source,
            LeadScore = client.LeadScore,
            Notes = client.Notes,
            CompanyName = client.CompanyName,
            Preferences = client.Preferences,
            CreatedAt = client.CreatedAt,
            LeadCount = client.Leads?.Count ?? 0,
            DealCount = client.Deals?.Count ?? 0
        };
    }
}
