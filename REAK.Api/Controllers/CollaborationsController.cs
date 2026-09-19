using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Collaboration;
using REAK.Api.Services.Security;
using REAK.Api.Services.Storage;

namespace REAK.Api.Controllers;

/// <summary>Workspace-scoped collaboration surface (spec §13). Every table behind these endpoints
/// carries RLS (fn_CollaborationParticipantPredicate) so a non-participant's queries simply return
/// no rows / a null result — reported here as 404, never a distinguishable 403, so a non-participant
/// can't even confirm the workspace exists. See CollaborationWorkspaceService for the write-guard
/// pattern that turns an RLS block-predicate violation into a 403 for the rare case (e.g. a stale
/// JWT) where RLS and the C# permission check briefly disagree.</summary>
[ApiController]
[Route("api/collaborations")]
[Authorize]
public class CollaborationsController(ICollaborationWorkspaceService workspaceService, IFileStorage fileStorage) : ControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    [HttpGet("{id:guid}")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var workspace = await workspaceService.GetAsync(id, ct);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpGet("{id:guid}/messages")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListMessages(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListMessagesAsync(id, ct));
    }

    [HttpPost("{id:guid}/messages")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> SendMessage(Guid id, SendMessageRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.SendMessageAsync(id, caller, request.Body, ct);
        return ToActionResult(op);
    }

    [HttpGet("{id:guid}/notes")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListNotes(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListNotesAsync(id, ct));
    }

    [HttpPost("{id:guid}/notes")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> AddNote(Guid id, AddNoteRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.AddNoteAsync(id, caller, request.Body, ct);
        return ToActionResult(op);
    }

    [HttpGet("{id:guid}/tasks")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListTasks(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListTasksAsync(id, ct));
    }

    [HttpPost("{id:guid}/tasks")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> CreateTask(Guid id, CreateTaskRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.CreateTaskAsync(id, caller, request.Title, request.AssignedToProfileId, request.DueDate, ct);
        return ToActionResult(op);
    }

    [HttpPut("{id:guid}/tasks/{taskId:guid}/status")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, Guid taskId, UpdateTaskStatusRequest request, CancellationToken ct)
    {
        var op = await workspaceService.UpdateTaskStatusAsync(id, taskId, request.Status, ct);
        return ToActionResult(op);
    }

    [HttpGet("{id:guid}/viewings")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListViewings(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListViewingsAsync(id, ct));
    }

    [HttpPost("{id:guid}/viewings")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> ScheduleViewing(Guid id, ScheduleViewingRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.ScheduleViewingAsync(id, caller, request.ScheduledAt, request.Notes, ct);
        return ToActionResult(op);
    }

    [HttpGet("{id:guid}/files")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListFiles(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListFilesAsync(id, ct));
    }

    [HttpPost("{id:guid}/files")]
    [RequirePermission("collaboration.create")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> UploadFile(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { error = "File is empty." });

        var caller = CallerContextFactory.From(User);
        await using var stream = file.OpenReadStream();
        var stored = await fileStorage.SaveAsync("collaboration-files", file.FileName, stream, ct);

        var (op, fileId) = await workspaceService.AddFileAsync(id, caller, file.FileName, stored.StoragePath, ct);
        if (op.Result != CollabOpResult.Success)
        {
            await fileStorage.DeleteAsync(stored.StoragePath, ct);
            return ToActionResult(op);
        }

        return Ok(new { id = fileId });
    }

    [HttpGet("{id:guid}/files/{fileId:guid}/download")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> DownloadFile(Guid id, Guid fileId, CancellationToken ct)
    {
        var (op, storagePath) = await workspaceService.GetFileForDownloadAsync(id, fileId, ct);
        if (op.Result != CollabOpResult.Success || storagePath is null) return ToActionResult(op);

        var stream = await fileStorage.OpenReadAsync(storagePath, ct);
        if (stream is null) return NotFound();

        return File(stream, "application/octet-stream", Path.GetFileName(storagePath));
    }

    [HttpDelete("{id:guid}/files/{fileId:guid}")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> DeleteFile(Guid id, Guid fileId, CancellationToken ct)
    {
        var (op, storagePath) = await workspaceService.DeleteFileAsync(id, fileId, ct);
        if (op.Result == CollabOpResult.Success && storagePath is not null)
        {
            await fileStorage.DeleteAsync(storagePath, ct);
        }
        return ToActionResult(op);
    }

    [HttpGet("{id:guid}/activities")]
    [RequirePermission("collaboration.read")]
    public async Task<IActionResult> ListActivities(Guid id, CancellationToken ct)
    {
        if (!await workspaceService.WorkspaceVisibleAsync(id, ct)) return NotFound();
        return Ok(await workspaceService.ListActivitiesAsync(id, ct));
    }

    [HttpPost("{id:guid}/contact-disclosures")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> GrantContactDisclosure(Guid id, GrantContactDisclosureRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.GrantContactDisclosureAsync(id, caller, request.DataType, ct);
        return ToActionResult(op);
    }

    [HttpDelete("{id:guid}/contact-disclosures/{disclosureId:guid}")]
    [RequirePermission("collaboration.create")]
    public async Task<IActionResult> RevokeContactDisclosure(Guid id, Guid disclosureId, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);
        var op = await workspaceService.RevokeContactDisclosureAsync(id, disclosureId, caller, ct);
        return ToActionResult(op);
    }

    private IActionResult ToActionResult(CollabOp op) => op.Result switch
    {
        CollabOpResult.Success => NoContent(),
        CollabOpResult.NotFound => NotFound(),
        CollabOpResult.Forbidden => Forbid(),
        CollabOpResult.InvalidState => BadRequest(new { error = op.Error }),
        _ => BadRequest(new { error = op.Error ?? "Request failed." }),
    };
}
