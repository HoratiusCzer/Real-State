using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace REAK.API.Models.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [MaxLength(255)]
    public string? FullName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<Branch> ManagedBranches { get; set; } = new List<Branch>();
    public virtual Client? ClientProfile { get; set; }
    public virtual ICollection<Lead> AssignedLeads { get; set; } = new List<Lead>();
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
    public virtual ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
}
