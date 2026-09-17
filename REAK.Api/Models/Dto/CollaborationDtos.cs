using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Dto;

public record CreateCollaborationRequestFromMatchRequest([Required] Guid MatchId, [MaxLength(1000)] string? Message);
public record CreateCollaborationRequestToOrgRequest([Required] Guid ToMemberEntityId, [MaxLength(1000)] string? Message);

public record CollaborationRequestDto(
    Guid Id,
    Guid? MatchId,
    Guid FromMemberEntityId,
    string FromMemberEntityName,
    Guid ToMemberEntityId,
    string ToMemberEntityName,
    string RequestedByProfileName,
    string Status,
    string? Message,
    DateTime? RespondedAt,
    DateTime CreatedAt,
    Guid? WorkspaceId);

public record CollaborationParticipantDto(Guid ProfileId, string ProfileName, Guid MemberEntityId, string MemberEntityName, DateTime JoinedAt);
public record CollaborationMessageDto(Guid Id, string SenderProfileName, string Body, DateTime CreatedAt);
public record CollaborationNoteDto(Guid Id, string AuthorProfileName, string Body, DateTime CreatedAt);
public record CollaborationTaskDto(Guid Id, string Title, string? AssignedToProfileName, DateTime? DueDate, string Status, DateTime CreatedAt);
public record CollaborationViewingDto(Guid Id, string ScheduledByProfileName, DateTime ScheduledAt, string? Notes, DateTime CreatedAt);
public record CollaborationFileDto(Guid Id, string FileName, string UploadedByProfileName, DateTime CreatedAt);
public record CollaborationActivityDto(Guid Id, string? ProfileName, string Action, DateTime CreatedAt);
public record CollaborationContactDisclosureDto(
    Guid Id, ContactDataType DataType, Guid GrantingMemberEntityId, string GrantingMemberEntityName,
    Guid ReceivingMemberEntityId, string ReceivingMemberEntityName, string GrantingProfileName,
    DateTime GrantedAt, DateTime? RevokedAt, string PolicyVersion);

public record CollaborationWorkspaceDto(
    Guid Id,
    Guid CollaborationRequestId,
    Guid? MatchId,
    Guid? ListingId,
    string? ListingTitle,
    Guid? DemandId,
    string? DemandTitle,
    DateTime CreatedAt,
    IReadOnlyList<CollaborationParticipantDto> Participants,
    IReadOnlyList<CollaborationContactDisclosureDto> ContactDisclosures);

public record CreateTaskRequest([Required, MaxLength(500)] string Title, Guid? AssignedToProfileId, DateTime? DueDate);
public record UpdateTaskStatusRequest([Required] CollaborationTaskStatus Status);
public record ScheduleViewingRequest([Required] DateTime ScheduledAt, [MaxLength(1000)] string? Notes);
public record SendMessageRequest([Required, MaxLength(4000)] string Body);
public record AddNoteRequest([Required, MaxLength(4000)] string Body);
public record GrantContactDisclosureRequest([Required] ContactDataType DataType);
