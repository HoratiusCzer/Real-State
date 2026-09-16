using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.API.Models.DTOs;
using REAK.API.Services;

namespace REAK.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AgentOrAbove")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    /// <summary>
    /// Search and list clients with filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ClientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ClientResponse>>> Search([FromQuery] ClientSearchRequest request)
    {
        var result = await _clientService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get a client by id
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        return Ok(client);
    }

    /// <summary>
    /// Get a client dashboard summary: lead counts by status, deal count, and recent leads
    /// </summary>
    [HttpGet("{id:int}/summary")]
    [ProducesResponseType(typeof(ClientSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientSummaryResponse>> GetSummary(int id)
    {
        var summary = await _clientService.GetSummaryAsync(id);
        return Ok(summary);
    }

    /// <summary>
    /// Create a new client. If no user account exists for the email, one is created with role Client.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] CreateClientRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>
    /// Update an existing client's profile
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientResponse>> Update(int id, [FromBody] UpdateClientRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.UpdateAsync(id, request);
        return Ok(client);
    }

    /// <summary>
    /// Delete a client (blocked if the client has existing deals)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _clientService.DeleteAsync(id);
        return NoContent();
    }
}
