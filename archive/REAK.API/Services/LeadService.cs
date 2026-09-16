using Microsoft.EntityFrameworkCore;
using REAK.API.Data;
using REAK.API.Middleware;
using REAK.API.Models.DTOs;
using REAK.API.Models.Entities;
using REAK.API.Models.Enums;

namespace REAK.API.Services;

public class LeadService : ILeadService
{
    private readonly ReakDbContext _context;
    private readonly ILogger<LeadService> _logger;

    public LeadService(ReakDbContext context, ILogger<LeadService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<LeadResponse>> SearchAsync(LeadSearchRequest request)
    {
        var query = _context.Leads
            .Include(l => l.Client)
            .Include(l => l.Property)
            .Include(l => l.AssignedAgent)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        if (request.Source.HasValue)
        {
            query = query.Where(l => l.Source == request.Source.Value);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(l => l.ClientId == request.ClientId.Value);
        }

        if (request.PropertyId.HasValue)
        {
            query = query.Where(l => l.PropertyId == request.PropertyId.Value);
        }

        if (request.AssignedAgentId.HasValue)
        {
            query = query.Where(l => l.AssignedAgentId == request.AssignedAgentId.Value);
        }

        if (request.Unassigned == true)
        {
            query = query.Where(l => l.AssignedAgentId == null);
        }

        if (request.MinBudget.HasValue)
        {
            query = query.Where(l => l.Budget >= request.MinBudget.Value);
        }

        if (request.MaxBudget.HasValue)
        {
            query = query.Where(l => l.Budget <= request.MaxBudget.Value);
        }

        if (request.FollowUpFrom.HasValue)
        {
            query = query.Where(l => l.FollowUpDate >= request.FollowUpFrom.Value);
        }

        if (request.FollowUpTo.HasValue)
        {
            query = query.Where(l => l.FollowUpDate <= request.FollowUpTo.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(l => l.Priority == request.Priority.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "followupdate" => request.SortDescending ? query.OrderByDescending(l => l.FollowUpDate) : query.OrderBy(l => l.FollowUpDate),
            "priority" => request.SortDescending ? query.OrderByDescending(l => l.Priority) : query.OrderBy(l => l.Priority),
            "budget" => request.SortDescending ? query.OrderByDescending(l => l.Budget) : query.OrderBy(l => l.Budget),
            _ => request.SortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var leads = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var data = leads.Select(MapToResponse).ToList();

        return PaginatedResponse<LeadResponse>.Create(data, request.Page, request.PageSize, totalCount);
    }

    public async Task<LeadResponse> GetByIdAsync(int id)
    {
        var lead = await GetLeadOrThrow(id);
        return MapToResponse(lead);
    }

    public async Task<LeadResponse> CreateAsync(CreateLeadRequest request)
    {
        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists)
        {
            throw new BadRequestException($"Client with id '{request.ClientId}' does not exist");
        }

        if (request.PropertyId.HasValue)
        {
            var propertyExists = await _context.Properties.AnyAsync(p => p.Id == request.PropertyId.Value);
            if (!propertyExists)
            {
                throw new BadRequestException($"Property with id '{request.PropertyId}' does not exist");
            }
        }

        if (request.AssignedAgentId.HasValue)
        {
            await ValidateAgentAsync(request.AssignedAgentId.Value);
        }

        var lead = new Lead
        {
            ClientId = request.ClientId,
            PropertyId = request.PropertyId,
            Source = request.Source,
            Status = LeadStatus.New,
            AssignedAgentId = request.AssignedAgentId,
            FollowUpDate = request.FollowUpDate,
            Notes = request.Notes,
            Budget = request.Budget,
            Requirements = request.Requirements,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Lead created: {LeadId} for client {ClientId}", lead.Id, lead.ClientId);

        return await GetByIdAsync(lead.Id);
    }

    public async Task<LeadResponse> UpdateAsync(int id, UpdateLeadRequest request)
    {
        var lead = await GetLeadOrThrow(id);

        if (request.PropertyId.HasValue)
        {
            var propertyExists = await _context.Properties.AnyAsync(p => p.Id == request.PropertyId.Value);
            if (!propertyExists)
            {
                throw new BadRequestException($"Property with id '{request.PropertyId}' does not exist");
            }
        }

        lead.PropertyId = request.PropertyId;
        lead.FollowUpDate = request.FollowUpDate;
        lead.Notes = request.Notes;
        lead.Budget = request.Budget;
        lead.Requirements = request.Requirements;
        lead.Priority = request.Priority;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Lead updated: {LeadId}", lead.Id);

        return await GetByIdAsync(lead.Id);
    }

    public async Task<LeadResponse> UpdateStatusAsync(int id, UpdateLeadStatusRequest request)
    {
        var lead = await GetLeadOrThrow(id);

        lead.Status = request.Status;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Lead {LeadId} status updated to {Status}", id, request.Status);

        return await GetByIdAsync(id);
    }

    public async Task<LeadResponse> AssignAsync(int id, AssignLeadRequest request)
    {
        var lead = await GetLeadOrThrow(id);

        await ValidateAgentAsync(request.AgentId);

        lead.AssignedAgentId = request.AgentId;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Lead {LeadId} assigned to agent {AgentId}", id, request.AgentId);

        return await GetByIdAsync(id);
    }

    public async Task<LeadResponse> UnassignAsync(int id)
    {
        var lead = await GetLeadOrThrow(id);

        lead.AssignedAgentId = null;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<LeadResponse> ScheduleFollowUpAsync(int id, UpdateLeadFollowUpRequest request)
    {
        var lead = await GetLeadOrThrow(id);

        lead.FollowUpDate = request.FollowUpDate;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var lead = await GetLeadOrThrow(id);

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Lead deleted: {LeadId}", id);
    }

    public async Task<LeadStatsResponse> GetStatsAsync()
    {
        var leads = await _context.Leads.ToListAsync();

        var byStatus = leads.GroupBy(l => l.Status).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var bySource = leads.GroupBy(l => l.Source).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var wonCount = leads.Count(l => l.Status == LeadStatus.Won);
        var lostCount = leads.Count(l => l.Status == LeadStatus.Lost);
        var closedCount = wonCount + lostCount;
        var now = DateTime.UtcNow;

        return new LeadStatsResponse
        {
            TotalLeads = leads.Count,
            ByStatus = byStatus,
            BySource = bySource,
            WonCount = wonCount,
            LostCount = lostCount,
            ConversionRate = closedCount == 0 ? 0 : Math.Round((double)wonCount / closedCount * 100, 2),
            UnassignedCount = leads.Count(l => l.AssignedAgentId == null),
            OverdueFollowUpCount = leads.Count(l => l.FollowUpDate.HasValue && l.FollowUpDate.Value < now
                && l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost)
        };
    }

    private async Task<Lead> GetLeadOrThrow(int id)
    {
        var lead = await _context.Leads
            .Include(l => l.Client)
            .Include(l => l.Property)
            .Include(l => l.AssignedAgent)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lead == null)
        {
            throw new NotFoundException("Lead", id);
        }

        return lead;
    }

    private async System.Threading.Tasks.Task ValidateAgentAsync(int agentId)
    {
        var agent = await _context.Users.FirstOrDefaultAsync(u => u.Id == agentId);
        if (agent == null)
        {
            throw new BadRequestException($"User with id '{agentId}' does not exist");
        }

        if (agent.Role == UserRole.Client)
        {
            throw new BadRequestException("Leads can only be assigned to Agent, Manager, Admin, or SuperAdmin users");
        }
    }

    public static LeadResponse MapToResponse(Lead lead)
    {
        return new LeadResponse
        {
            Id = lead.Id,
            ClientId = lead.ClientId,
            ClientName = lead.Client?.Name,
            ClientPhone = lead.Client?.Phone,
            PropertyId = lead.PropertyId,
            PropertyTitle = lead.Property?.Title,
            Status = lead.Status,
            Source = lead.Source,
            AssignedAgentId = lead.AssignedAgentId,
            AssignedAgentName = lead.AssignedAgent?.FullName,
            FollowUpDate = lead.FollowUpDate,
            Notes = lead.Notes,
            Budget = lead.Budget,
            Requirements = lead.Requirements,
            Priority = lead.Priority,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
