using REAK.Api.Models.Dto;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Collaboration;

public interface ICollaborationWorkspaceService
{
    Task<CollaborationWorkspaceDto?> GetAsync(Guid workspaceId, CancellationToken ct = default);
    Task<List<CollaborationMessageDto>> ListMessagesAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollabOp> SendMessageAsync(Guid workspaceId, CallerContext caller, string body, CancellationToken ct = default);
    Task<List<CollaborationNoteDto>> ListNotesAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollabOp> AddNoteAsync(Guid workspaceId, CallerContext caller, string body, CancellationToken ct = default);
    Task<List<CollaborationTaskDto>> ListTasksAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollabOp> CreateTaskAsync(Guid workspaceId, CallerContext caller, string title, Guid? assignedToProfileId, DateTime? dueDate, CancellationToken ct = default);
    Task<CollabOp> UpdateTaskStatusAsync(Guid workspaceId, Guid taskId, CollaborationTaskStatus status, CancellationToken ct = default);
    Task<List<CollaborationViewingDto>> ListViewingsAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollabOp> ScheduleViewingAsync(Guid workspaceId, CallerContext caller, DateTime scheduledAt, string? notes, CancellationToken ct = default);
    Task<List<CollaborationFileDto>> ListFilesAsync(Guid workspaceId, CancellationToken ct = default);
    Task<(CollabOp Op, Guid? FileId)> AddFileAsync(Guid workspaceId, CallerContext caller, string fileName, string storagePath, CancellationToken ct = default);
    Task<(CollabOp Op, string? StoragePath)> GetFileForDownloadAsync(Guid workspaceId, Guid fileId, CancellationToken ct = default);
    Task<(CollabOp Op, string? StoragePath)> DeleteFileAsync(Guid workspaceId, Guid fileId, CancellationToken ct = default);
    Task<List<CollaborationActivityDto>> ListActivitiesAsync(Guid workspaceId, CancellationToken ct = default);
    Task<CollabOp> GrantContactDisclosureAsync(Guid workspaceId, CallerContext caller, ContactDataType dataType, CancellationToken ct = default);
    Task<CollabOp> RevokeContactDisclosureAsync(Guid workspaceId, Guid disclosureId, CallerContext caller, CancellationToken ct = default);
}
