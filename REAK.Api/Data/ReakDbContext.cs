using Microsoft.EntityFrameworkCore;
using REAK.Api.Models.Entities.Audit;
using REAK.Api.Models.Entities.Cms;
using REAK.Api.Models.Entities.Collaboration;
using REAK.Api.Models.Entities.Demands;
using REAK.Api.Models.Entities.FeatureFlags;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Entities.Matching;
using REAK.Api.Models.Entities.Notifications;
using REAK.Api.Models.Entities.Reference;
using CmsEvent = REAK.Api.Models.Entities.Cms.Event;

namespace REAK.Api.Data;

public class ReakDbContext : DbContext
{
    public ReakDbContext(DbContextOptions<ReakDbContext> options) : base(options)
    {
    }

    // Identity & tenancy
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<MemberEntity> MemberEntities { get; set; } = null!;
    public DbSet<EntityUser> EntityUsers { get; set; } = null!;
    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<MembershipApplication> MembershipApplications { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<ProfileRoleAssignment> ProfileRoleAssignments { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

    // Reference data
    public DbSet<Province> Provinces { get; set; } = null!;
    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<Municipality> Municipalities { get; set; } = null!;
    public DbSet<Ward> Wards { get; set; } = null!;
    public DbSet<Locality> Localities { get; set; } = null!;
    public DbSet<AreaUnit> AreaUnits { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<PropertyType> PropertyTypes { get; set; } = null!;
    public DbSet<PropertySubtype> PropertySubtypes { get; set; } = null!;
    public DbSet<Purpose> Purposes { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;

    // Property exchange
    public DbSet<PropertyListing> PropertyListings { get; set; } = null!;
    public DbSet<ListingMedia> ListingMedia { get; set; } = null!;
    public DbSet<ListingDocument> ListingDocuments { get; set; } = null!;
    public DbSet<ListingAmenity> ListingAmenities { get; set; } = null!;
    public DbSet<ListingContact> ListingContacts { get; set; } = null!;
    public DbSet<ListingVisibilityMember> ListingVisibilityMembers { get; set; } = null!;

    // Demands
    public DbSet<Demand> Demands { get; set; } = null!;
    public DbSet<DemandPropertyType> DemandPropertyTypes { get; set; } = null!;
    public DbSet<DemandLocation> DemandLocations { get; set; } = null!;
    public DbSet<DemandAmenity> DemandAmenities { get; set; } = null!;
    public DbSet<DemandContact> DemandContacts { get; set; } = null!;
    public DbSet<DemandVisibilityMember> DemandVisibilityMembers { get; set; } = null!;

    // Matching
    public DbSet<MatchRuleSet> MatchRuleSets { get; set; } = null!;
    public DbSet<MatchRule> MatchRules { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<MatchComponent> MatchComponents { get; set; } = null!;
    public DbSet<MatchAction> MatchActions { get; set; } = null!;

    // Collaboration
    public DbSet<CollaborationRequest> CollaborationRequests { get; set; } = null!;
    public DbSet<CollaborationWorkspace> CollaborationWorkspaces { get; set; } = null!;
    public DbSet<CollaborationParticipant> CollaborationParticipants { get; set; } = null!;
    public DbSet<CollaborationMessage> CollaborationMessages { get; set; } = null!;
    public DbSet<CollaborationFile> CollaborationFiles { get; set; } = null!;
    public DbSet<CollaborationNote> CollaborationNotes { get; set; } = null!;
    public DbSet<CollaborationTask> CollaborationTasks { get; set; } = null!;
    public DbSet<CollaborationViewing> CollaborationViewings { get; set; } = null!;
    public DbSet<CollaborationActivity> CollaborationActivities { get; set; } = null!;
    public DbSet<CollaborationContactDisclosure> CollaborationContactDisclosures { get; set; } = null!;

    // Notifications, feature flags, audit
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<FeatureFlag> FeatureFlags { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    // CMS
    public DbSet<CmsPage> CmsPages { get; set; } = null!;
    public DbSet<NewsArticle> NewsArticles { get; set; } = null!;
    public DbSet<Notice> Notices { get; set; } = null!;
    public DbSet<CmsEvent> Events { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<CommitteeMember> CommitteeMembers { get; set; } = null!;
    public DbSet<NavigationItem> NavigationItems { get; set; } = null!;
    public DbSet<SiteSetting> SiteSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Conservative baseline: every foreign key defaults to RESTRICT. EF Core's convention for a
        // required relationship is CASCADE, which is the wrong default for this schema (we do not want
        // deleting a Profile or MemberEntity to silently cascade-delete listings, demands, or audit
        // history). Cascade is opted into explicitly below only for true parent-owns-child aggregates.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        ConfigureIdentity(modelBuilder);
        ConfigureReference(modelBuilder);
        ConfigureListings(modelBuilder);
        ConfigureDemands(modelBuilder);
        ConfigureMatching(modelBuilder);
        ConfigureCollaboration(modelBuilder);
        ConfigureNotificationsFlagsAudit(modelBuilder);
        ConfigureCms(modelBuilder);
        ConfigureCheckConstraints(modelBuilder);
    }

    /// <summary>Spec §21 (Database Quality Standards) calls for check constraints, not just
    /// application-side validation. These are the non-negotiable numeric bounds — values that are
    /// invalid under any business rule, not judgment calls that belong in a service layer.</summary>
    private static void ConfigureCheckConstraints(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropertyListing>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_Price", "[Price] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_LandArea", "[LandArea] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_BuiltUpArea", "[BuiltUpArea] IS NULL OR [BuiltUpArea] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_Bedrooms", "[Bedrooms] IS NULL OR [Bedrooms] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_Bathrooms", "[Bathrooms] IS NULL OR [Bathrooms] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_Floors", "[Floors] IS NULL OR [Floors] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PropertyListings_ParkingSpaces", "[ParkingSpaces] IS NULL OR [ParkingSpaces] >= 0"));
        });

        modelBuilder.Entity<Demand>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MinBudget", "[MinBudget] IS NULL OR [MinBudget] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MaxBudget", "[MaxBudget] IS NULL OR [MaxBudget] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_BudgetRange", "[MinBudget] IS NULL OR [MaxBudget] IS NULL OR [MaxBudget] >= [MinBudget]"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MinArea", "[MinArea] IS NULL OR [MinArea] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MaxArea", "[MaxArea] IS NULL OR [MaxArea] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_AreaRange", "[MinArea] IS NULL OR [MaxArea] IS NULL OR [MaxArea] >= [MinArea]"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MinBedrooms", "[MinBedrooms] IS NULL OR [MinBedrooms] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Demands_MinBathrooms", "[MinBathrooms] IS NULL OR [MinBathrooms] >= 0"));
        });

        modelBuilder.Entity<MatchRule>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_MatchRules_Weight", "[Weight] >= 0"));
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Matches_Score", "[Score] >= 0"));
        });
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<EntityUser>(entity =>
        {
            entity.HasIndex(e => new { e.ProfileId, e.MemberEntityId }).IsUnique();

            entity.HasOne(e => e.Profile)
                .WithMany(p => p.EntityMemberships)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileRoleAssignment>(entity =>
        {
            entity.HasOne(e => e.Profile)
                .WithMany(p => p.RoleAssignments)
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.ProfileRoleAssignments)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
        });

        modelBuilder.Entity<MembershipApplication>(entity =>
        {
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.ProfileId);

            entity.HasOne(e => e.Profile)
                .WithMany()
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.ProfileId);

            entity.HasOne(e => e.Profile)
                .WithMany()
                .HasForeignKey(e => e.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureReference(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<District>()
            .HasOne(d => d.Province).WithMany(p => p.Districts)
            .HasForeignKey(d => d.ProvinceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Municipality>()
            .HasOne(m => m.District).WithMany(d => d.Municipalities)
            .HasForeignKey(m => m.DistrictId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ward>()
            .HasOne(w => w.Municipality).WithMany(m => m.Wards)
            .HasForeignKey(w => w.MunicipalityId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Locality>()
            .HasOne(l => l.Ward).WithMany(w => w.Localities)
            .HasForeignKey(l => l.WardId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PropertySubtype>()
            .HasOne(s => s.PropertyType).WithMany(t => t.Subtypes)
            .HasForeignKey(s => s.PropertyTypeId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Currency>().HasIndex(e => e.Code).IsUnique();
    }

    private static void ConfigureListings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropertyListing>(entity =>
        {
            entity.HasIndex(e => e.ReferenceCode).IsUnique();
            entity.HasIndex(e => new { e.Status, e.NetworkVisibility });
            entity.HasIndex(e => e.MemberEntityId);

            entity.HasOne(e => e.MemberEntity).WithMany(m => m.Listings)
                .HasForeignKey(e => e.MemberEntityId);
        });

        modelBuilder.Entity<ListingMedia>()
            .HasOne(e => e.Listing).WithMany(l => l.Media)
            .HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingDocument>()
            .HasOne(e => e.Listing).WithMany(l => l.Documents)
            .HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingAmenity>(entity =>
        {
            entity.HasIndex(e => new { e.ListingId, e.AmenityId }).IsUnique();

            entity.HasOne(e => e.Listing).WithMany(l => l.ListingAmenities)
                .HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        // ListingContact: structurally isolated 1:1. Cascades with its listing (no independent
        // lifecycle) but is never included in a normal listing query path — callers must opt in
        // explicitly (see docs/REAK-requirements.md §2.3, §8.4).
        modelBuilder.Entity<ListingContact>()
            .HasOne(e => e.Listing).WithOne(l => l.Contact)
            .HasForeignKey<ListingContact>(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingVisibilityMember>(entity =>
        {
            entity.HasIndex(e => new { e.ListingId, e.MemberEntityId }).IsUnique();

            entity.HasOne(e => e.Listing).WithMany(l => l.VisibilityMembers)
                .HasForeignKey(e => e.ListingId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureDemands(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Demand>(entity =>
        {
            entity.HasIndex(e => e.ReferenceCode).IsUnique();
            entity.HasIndex(e => new { e.Status, e.NetworkVisibility });
            entity.HasIndex(e => e.MemberEntityId);

            entity.HasOne(e => e.MemberEntity).WithMany(m => m.Demands)
                .HasForeignKey(e => e.MemberEntityId);
        });

        modelBuilder.Entity<DemandPropertyType>(entity =>
        {
            entity.HasIndex(e => new { e.DemandId, e.PropertyTypeId }).IsUnique();

            entity.HasOne(e => e.Demand).WithMany(d => d.PropertyTypes)
                .HasForeignKey(e => e.DemandId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DemandLocation>()
            .HasOne(e => e.Demand).WithMany(d => d.Locations)
            .HasForeignKey(e => e.DemandId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DemandAmenity>(entity =>
        {
            entity.HasIndex(e => new { e.DemandId, e.AmenityId }).IsUnique();

            entity.HasOne(e => e.Demand).WithMany(d => d.DemandAmenities)
                .HasForeignKey(e => e.DemandId).OnDelete(DeleteBehavior.Cascade);
        });

        // DemandContact: structurally isolated 1:1, same rationale as ListingContact.
        modelBuilder.Entity<DemandContact>()
            .HasOne(e => e.Demand).WithOne(d => d.Contact)
            .HasForeignKey<DemandContact>(e => e.DemandId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DemandVisibilityMember>(entity =>
        {
            entity.HasIndex(e => new { e.DemandId, e.MemberEntityId }).IsUnique();

            entity.HasOne(e => e.Demand).WithMany(d => d.VisibilityMembers)
                .HasForeignKey(e => e.DemandId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMatching(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchRuleSet>()
            .HasIndex(e => new { e.Name, e.Version }).IsUnique();

        modelBuilder.Entity<MatchRule>()
            .HasOne(e => e.MatchRuleSet).WithMany(rs => rs.Rules)
            .HasForeignKey(e => e.MatchRuleSetId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasIndex(e => new { e.ListingId, e.DemandId }).IsUnique();

            entity.HasOne(e => e.Listing).WithMany()
                .HasForeignKey(e => e.ListingId);

            entity.HasOne(e => e.Demand).WithMany()
                .HasForeignKey(e => e.DemandId);
        });

        modelBuilder.Entity<MatchComponent>()
            .HasOne(e => e.Match).WithMany(m => m.Components)
            .HasForeignKey(e => e.MatchId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchAction>()
            .HasOne(e => e.Match).WithMany(m => m.Actions)
            .HasForeignKey(e => e.MatchId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureCollaboration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CollaborationWorkspace>()
            .HasIndex(e => e.CollaborationRequestId).IsUnique();

        modelBuilder.Entity<CollaborationWorkspace>()
            .HasOne(e => e.Request).WithOne(r => r.Workspace)
            .HasForeignKey<CollaborationWorkspace>(e => e.CollaborationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationParticipant>(entity =>
        {
            entity.HasIndex(e => new { e.CollaborationWorkspaceId, e.ProfileId }).IsUnique();

            entity.HasOne(e => e.Workspace).WithMany(w => w.Participants)
                .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollaborationMessage>()
            .HasOne(e => e.Workspace).WithMany(w => w.Messages)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationFile>()
            .HasOne(e => e.Workspace).WithMany(w => w.Files)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationNote>()
            .HasOne(e => e.Workspace).WithMany(w => w.Notes)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationTask>()
            .HasOne(e => e.Workspace).WithMany(w => w.Tasks)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationViewing>()
            .HasOne(e => e.Workspace).WithMany(w => w.Viewings)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationActivity>()
            .HasOne(e => e.Workspace).WithMany(w => w.Activities)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollaborationContactDisclosure>()
            .HasOne(e => e.Workspace).WithMany(w => w.ContactDisclosures)
            .HasForeignKey(e => e.CollaborationWorkspaceId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureNotificationsFlagsAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.ProfileId, e.IsRead });

            entity.HasOne(e => e.Profile).WithMany(p => p.Notifications)
                .HasForeignKey(e => e.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeatureFlag>().HasIndex(e => e.Key).IsUnique();

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private static void ConfigureCms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CmsPage>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<NewsArticle>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<Notice>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<CmsEvent>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<SiteSetting>().HasIndex(e => e.Key).IsUnique();

        modelBuilder.Entity<NavigationItem>()
            .HasOne(e => e.Parent).WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
