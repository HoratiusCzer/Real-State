using Microsoft.EntityFrameworkCore;
using REAK.API.Models.Entities;

namespace REAK.API.Data;

public class ReakDbContext : DbContext
{
    public ReakDbContext(DbContextOptions<ReakDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Deal> Deals { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Commission> Commissions { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Models.Entities.Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        // Branch configurations
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasOne(b => b.Manager)
                .WithMany(u => u.ManagedBranches)
                .HasForeignKey(b => b.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Property configurations
        modelBuilder.Entity<Property>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(p => p.Branch)
                .WithMany(b => b.Properties)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PropertyImage configurations
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Client configurations
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Phone);
            entity.Property(e => e.Source).HasConversion<int>();

            entity.HasOne(c => c.User)
                .WithOne(u => u.ClientProfile)
                .HasForeignKey<Client>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Lead configurations
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Source).HasConversion<int>();

            entity.HasOne(l => l.Client)
                .WithMany(c => c.Leads)
                .HasForeignKey(l => l.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Property)
                .WithMany(p => p.Leads)
                .HasForeignKey(l => l.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.AssignedAgent)
                .WithMany(u => u.AssignedLeads)
                .HasForeignKey(l => l.AssignedAgentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Deal configurations
        modelBuilder.Entity<Deal>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasIndex(e => e.ContractNumber).IsUnique();

            entity.HasOne(d => d.Property)
                .WithMany(p => p.Deals)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Client)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Agent)
                .WithMany(u => u.Deals)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction configurations
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.PaymentMethod).HasConversion<int>();

            entity.HasOne(t => t.Deal)
                .WithMany(d => d.Transactions)
                .HasForeignKey(t => t.DealId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Commission configurations
        modelBuilder.Entity<Commission>(entity =>
        {
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(c => c.Deal)
                .WithMany(d => d.Commissions)
                .HasForeignKey(c => c.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Agent)
                .WithMany(u => u.Commissions)
                .HasForeignKey(c => c.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Activity configurations
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification configurations
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<int>();
            entity.HasIndex(e => new { e.UserId, e.IsRead });

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Task configurations
        modelBuilder.Entity<Models.Entities.Task>(entity =>
        {
            entity.ToTable("Tasks");
            entity.Property(e => e.Priority).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
