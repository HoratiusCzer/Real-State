using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.API.Models.DTOs;
using REAK.API.Services;

namespace REAK.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertyController> _logger;

    public PropertyController(IPropertyService propertyService, ILogger<PropertyController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    /// <summary>
    /// Search and list properties with filters and pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedResponse<PropertyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<PropertyResponse>>> Search([FromQuery] PropertySearchRequest request)
    {
        var result = await _propertyService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get a property by id
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyResponse>> GetById(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        return Ok(property);
    }

    /// <summary>
    /// Create a new property
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PropertyResponse>> Create([FromBody] CreatePropertyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _propertyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyResponse>> Update(int id, [FromBody] UpdatePropertyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _propertyService.UpdateAsync(id, request);
        return Ok(property);
    }

    /// <summary>
    /// Update a property's status (e.g. Available, UnderOffer, Sold, Rented, OffMarket)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(PropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyResponse>> UpdateStatus(int id, [FromBody] UpdatePropertyStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await _propertyService.UpdateStatusAsync(id, request);
        return Ok(property);
    }

    /// <summary>
    /// Delete a property
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _propertyService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Upload one or more images for a property
    /// </summary>
    [HttpPost("{id:int}/images")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(List<PropertyImageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PropertyImageResponse>>> UploadImages(int id, [FromForm] List<IFormFile> files)
    {
        var images = await _propertyService.UploadImagesAsync(id, files);
        return CreatedAtAction(nameof(GetById), new { id }, images);
    }

    /// <summary>
    /// Delete an image from a property
    /// </summary>
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        await _propertyService.DeleteImageAsync(id, imageId);
        return NoContent();
    }

    /// <summary>
    /// Set an image as the primary image for a property
    /// </summary>
    [HttpPut("{id:int}/images/{imageId:int}/primary")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(PropertyImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyImageResponse>> SetPrimaryImage(int id, int imageId)
    {
        var image = await _propertyService.SetPrimaryImageAsync(id, imageId);
        return Ok(image);
    }
}
