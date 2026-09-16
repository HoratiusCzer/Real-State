using Microsoft.EntityFrameworkCore;
using REAK.Api.Models.Entities.FeatureFlags;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Entities.Reference;
using REAK.Api.Models.Enums;

namespace REAK.Api.Data;

/// <summary>Seeds only data that is either structural (roles/permissions/feature-flag keys) or
/// explicitly enumerated in the spec itself (spec §11 area units, spec §9 purposes, Nepal's 7
/// provinces — an established administrative fact, not a REAK business fact). Deliberately does
/// NOT seed property types, amenities, districts, municipalities, wards, or localities: the spec
/// marks those admin-configurable without giving an exhaustive list, and inventing one would be
/// exactly the kind of fabricated content spec §22 prohibits. It also never seeds a SuperAdmin
/// user account — that requires a real password set through a proper flow, which is Stage 4
/// (Authentication) territory, not a hardcoded seeded credential.</summary>
public static class DatabaseSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(ReakDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedPermissionsAsync(context);
        await SeedRolesAsync(context);
        await SeedFeatureFlagsAsync(context);
        await SeedAreaUnitsAsync(context);
        await SeedPurposesAsync(context);
        await SeedProvincesAsync(context);
        await SeedCurrenciesAsync(context);
    }

    private static readonly string[] PermissionSlugs =
    {
        "members.read", "members.create", "members.update", "members.suspend",
        "listings.read", "listings.create", "listings.update", "listings.moderate",
        "demands.read", "demands.create",
        "matches.read", "match_rules.manage",
        "collaboration.create", "collaboration.read",
        "cms.manage", "settings.manage", "audit.read",
    };

    private static async System.Threading.Tasks.Task SeedPermissionsAsync(ReakDbContext context)
    {
        foreach (var slug in PermissionSlugs)
        {
            if (!await context.Permissions.AnyAsync(p => p.Slug == slug))
            {
                context.Permissions.Add(new Permission { Id = Guid.NewGuid(), Slug = slug });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>Default role -> permission mapping. Not spec-mandated in this level of detail (the
    /// spec gives the permission list and the four role names but not the exact grant matrix) — a
    /// reasonable starting point, adjustable later via the Admin Portal (Stage 11).</summary>
    private static readonly Dictionary<string, string[]> RolePermissionMap = new()
    {
        ["SuperAdmin"] = PermissionSlugs,
        ["AssociationAdmin"] = PermissionSlugs,
        ["MemberAdmin"] = new[]
        {
            "members.read",
            "listings.read", "listings.create", "listings.update",
            "demands.read", "demands.create",
            "matches.read",
            "collaboration.create", "collaboration.read",
        },
        ["MemberStaff"] = new[]
        {
            "members.read",
            "listings.read", "listings.create",
            "demands.read", "demands.create",
            "matches.read",
            "collaboration.read",
        },
    };

    private static async System.Threading.Tasks.Task SeedRolesAsync(ReakDbContext context)
    {
        var roleDefinitions = new (string Name, RoleScope Scope)[]
        {
            ("SuperAdmin", RoleScope.System),
            ("AssociationAdmin", RoleScope.System),
            ("MemberAdmin", RoleScope.Organization),
            ("MemberStaff", RoleScope.Organization),
        };

        var permissionsBySlug = await context.Permissions.ToDictionaryAsync(p => p.Slug);

        foreach (var (name, scope) in roleDefinitions)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role == null)
            {
                role = new Role { Id = Guid.NewGuid(), Name = name, Scope = scope };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            var grantedSlugs = RolePermissionMap[name];
            var existingGrants = await context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            foreach (var slug in grantedSlugs)
            {
                var permission = permissionsBySlug[slug];
                if (!existingGrants.Contains(permission.Id))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>The 13 flags from spec §15, all off — "defaults should be conservative".</summary>
    private static readonly string[] FeatureFlagKeys =
    {
        "public_properties_enabled",
        "public_member_directory_enabled",
        "open_registration_enabled",
        "membership_application_enabled",
        "property_moderation_required",
        "matching_enabled",
        "collaboration_enabled",
        "deal_tracking_enabled",
        "auto_unit_conversion",
        "sms_notifications",
        "whatsapp_notifications",
        "email_notifications",
        "member_export_enabled",
    };

    private static async System.Threading.Tasks.Task SeedFeatureFlagsAsync(ReakDbContext context)
    {
        foreach (var key in FeatureFlagKeys)
        {
            if (!await context.FeatureFlags.AnyAsync(f => f.Key == key))
            {
                context.FeatureFlags.Add(new FeatureFlag { Id = Guid.NewGuid(), Key = key, IsEnabled = false });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>Spec §11's exact list. ConversionToSquareMeters is left null on every row —
    /// automatic conversion stays disabled until an admin explicitly configures and approves it.</summary>
    private static readonly string[] AreaUnitNames =
    {
        "Ropani", "Aana", "Paisa", "Dam", "Bigha", "Kattha", "Dhur", "Square Feet", "Square Metres",
    };

    private static async System.Threading.Tasks.Task SeedAreaUnitsAsync(ReakDbContext context)
    {
        for (var i = 0; i < AreaUnitNames.Length; i++)
        {
            var name = AreaUnitNames[i];
            if (!await context.AreaUnits.AnyAsync(u => u.Name == name))
            {
                context.AreaUnits.Add(new AreaUnit { Id = Guid.NewGuid(), Name = name, SortOrder = i });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>Spec §9's exact list: "buyer, tenant, investor, other".</summary>
    private static readonly string[] PurposeNames = { "Buyer", "Tenant", "Investor", "Other" };

    private static async System.Threading.Tasks.Task SeedPurposesAsync(ReakDbContext context)
    {
        for (var i = 0; i < PurposeNames.Length; i++)
        {
            var name = PurposeNames[i];
            if (!await context.Purposes.AnyAsync(p => p.Name == name))
            {
                context.Purposes.Add(new Purpose { Id = Guid.NewGuid(), Name = name, SortOrder = i });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>Nepal's 7 provinces — an established federal administrative fact, not a REAK
    /// business fact. Districts/municipalities/wards/localities are intentionally left empty; see
    /// the type-level doc comment.</summary>
    private static readonly string[] ProvinceNames =
    {
        "Koshi", "Madhesh", "Bagmati", "Gandaki", "Lumbini", "Karnali", "Sudurpashchim",
    };

    private static async System.Threading.Tasks.Task SeedProvincesAsync(ReakDbContext context)
    {
        for (var i = 0; i < ProvinceNames.Length; i++)
        {
            var name = ProvinceNames[i];
            if (!await context.Provinces.AnyAsync(p => p.Name == name))
            {
                context.Provinces.Add(new Province { Id = Guid.NewGuid(), Name = name, SortOrder = i });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async System.Threading.Tasks.Task SeedCurrenciesAsync(ReakDbContext context)
    {
        if (!await context.Currencies.AnyAsync(c => c.Code == "NPR"))
        {
            context.Currencies.Add(new Currency { Id = Guid.NewGuid(), Code = "NPR", Symbol = "Rs." });
        }

        await context.SaveChangesAsync();
    }
}
