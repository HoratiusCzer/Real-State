using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.API.Models.DTOs;
using REAK.API.Services;

namespace REAK.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AgentOrAbove")]
public class LeadController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ILogger<LeadController> _logger;

    public LeadController(ILeadService leadService, ILogger<LeadController> logger)
    {
        _leadService = leadService;
        _logger = logger;
    }

    /// <summary>
    /// Search and list leads with filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LeadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<LeadResponse>>> Search([FromQuery] LeadSearchRequest request)
    {
        var result = await _leadService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get lead pipeline statistics: counts by status/source, conversion rate, unassigned and overdue follow-ups
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(LeadStatsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadStatsResponse>> GetStats()
    {
        var stats = await _leadService.GetStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get a lead by id
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> GetById(int id)
    {
        var lead = await _leadService.GetByIdAsync(id);
        return Ok(lead);
    }

    /// <summary>
    /// Capture a new lead for a client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadResponse>> Create([FromBody] CreateLeadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var lead = await _leadService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, lead);
    }

    /// <summary>
    /// Update lead details (property interest, notes, budget, requirements, priority, follow-up)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> Update(int id, [FromBody] UpdateLeadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var lead = await _leadService.UpdateAsync(id, request);
        return Ok(lead);
    }

    /// <summary>
    /// Move a lead through the pipeline: New, Contacted, Qualified, Negotiation, Won, Lost
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> UpdateStatus(int id, [FromBody] UpdateLeadStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var lead = await _leadService.UpdateStatusAsync(id, request);
        return Ok(lead);
    }

    /// <summary>
    /// Assign a lead to an agent
    /// </summary>
    [HttpPatch("{id:int}/assign")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> Assign(int id, [FromBody] AssignLeadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var lead = await _leadService.AssignAsync(id, request);
        return Ok(lead);
    }

    /// <summary>
    /// Remove the assigned agent from a lead
    /// </summary>
    [HttpPatch("{id:int}/unassign")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> Unassign(int id)
    {
        var lead = await _leadService.UnassignAsync(id);
        return Ok(lead);
    }

    /// <summary>
    /// Schedule or reschedule a follow-up date for a lead
    /// </summary>
    [HttpPatch("{id:int}/follow-up")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadResponse>> ScheduleFollowUp(int id, [FromBody] UpdateLeadFollowUpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var lead = await _leadService.ScheduleFollowUpAsync(id, request);
        return Ok(lead);
    }

    /// <summary>
    /// Delete a lead
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _leadService.DeleteAsync(id);
        return NoContent();
    }
}
