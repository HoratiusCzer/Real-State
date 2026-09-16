using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>Created on acceptance of a CollaborationRequest (spec §2.4 Flow D step 3; maps to the
/// spec's `collaborations` table — named Workspace here to avoid colliding with the containing
/// namespace). Accessible only to authorized participants; contact info stays private until an
/// explicit CollaborationContactDisclosure grant exists.</summary>
public class CollaborationWorkspace
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Request))]
    public Guid CollaborationRequestId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationRequest Request { get; set; } = null!;
    public virtual ICollection<CollaborationParticipant> Participants { get; set; } = new List<CollaborationParticipant>();
    public virtual ICollection<CollaborationMessage> Messages { get; set; } = new List<CollaborationMessage>();
    public virtual ICollection<CollaborationFile> Files { get; set; } = new List<CollaborationFile>();
    public virtual ICollection<CollaborationNote> Notes { get; set; } = new List<CollaborationNote>();
    public virtual ICollection<CollaborationTask> Tasks { get; set; } = new List<CollaborationTask>();
    public virtual ICollection<CollaborationViewing> Viewings { get; set; } = new List<CollaborationViewing>();
    public virtual ICollection<CollaborationActivity> Activities { get; set; } = new List<CollaborationActivity>();
    public virtual ICollection<CollaborationContactDisclosure> ContactDisclosures { get; set; } = new List<CollaborationContactDisclosure>();
}
