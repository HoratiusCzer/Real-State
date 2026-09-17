using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Matching;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Admin authoring of match rule sets (spec §12 — "no hardcoded weights... admin
/// configures rules"). Full lifecycle management (editing a rule mid-flight, listing all rule
/// sets with pagination, etc.) is Stage 11 (Admin Portal) territory; this is the minimal slice
/// needed for the matching engine to have something to evaluate against at all — the same
/// "unblock the dependent feature, defer the full manage UI" pattern as Stage 6's
/// ReferenceDataController. A rule set is created Draft, rules are added to it, then it's
/// published — publishing is one-way here (no un-publish) because spec §12 requires matches to
/// record which rule set/version produced them; superseding is done by publishing a new version,
/// not mutating history.</summary>
[ApiController]
[Route("api/match-rule-sets")]
[Authorize]
[RequirePermission("match_rules.manage")]
public class MatchRuleSetsController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var sets = await db.MatchRuleSets
            .Include(s => s.Rules)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => ToDto(s))
            .ToListAsync(ct);
        return Ok(sets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var set = await db.MatchRuleSets.Include(s => s.Rules).FirstOrDefaultAsync(s => s.Id == id, ct);
        return set is null ? NotFound() : Ok(ToDto(set));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMatchRuleSetRequest request, CancellationToken ct)
    {
        var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var maxVersion = await db.MatchRuleSets.Where(s => s.Name == request.Name).Select(s => (int?)s.Version).MaxAsync(ct) ?? 0;

        var set = new MatchRuleSet
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Version = maxVersion + 1,
            Status = MatchRuleSetStatus.Draft,
            Description = request.Description,
            CreatedByProfileId = profileId,
        };
        db.MatchRuleSets.Add(set);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = set.Id }, ToDto(set));
    }

    [HttpPost("{id:guid}/rules")]
    public async Task<IActionResult> AddRule(Guid id, CreateMatchRuleRequest request, CancellationToken ct)
    {
        var set = await db.MatchRuleSets.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (set is null) return NotFound();
        if (set.Status != MatchRuleSetStatus.Draft) return BadRequest(new { error = "Only a draft rule set can have rules added." });

        var rule = new MatchRule
        {
            Id = Guid.NewGuid(),
            MatchRuleSetId = id,
            Criterion = request.Criterion,
            Weight = request.Weight,
            IsRequired = request.IsRequired,
            ToleranceValue = request.ToleranceValue,
            SortOrder = request.SortOrder,
        };
        db.MatchRules.Add(rule);
        await db.SaveChangesAsync(ct);
        return Ok(new MatchRuleDto(rule.Id, rule.Criterion, rule.Weight, rule.IsRequired, rule.ToleranceValue, rule.SortOrder));
    }

    [HttpDelete("{id:guid}/rules/{ruleId:guid}")]
    public async Task<IActionResult> RemoveRule(Guid id, Guid ruleId, CancellationToken ct)
    {
        var set = await db.MatchRuleSets.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (set is null) return NotFound();
        if (set.Status != MatchRuleSetStatus.Draft) return BadRequest(new { error = "Only a draft rule set can have rules removed." });

        var rule = await db.MatchRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.MatchRuleSetId == id, ct);
        if (rule is null) return NotFound();

        db.MatchRules.Remove(rule);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var set = await db.MatchRuleSets.Include(s => s.Rules).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (set is null) return NotFound();
        if (set.Status != MatchRuleSetStatus.Draft) return BadRequest(new { error = "Only a draft rule set can be published." });
        if (set.Rules.Count == 0) return BadRequest(new { error = "A rule set needs at least one rule before it can be published." });
        if (set.Rules.All(r => r.Weight == 0)) return BadRequest(new { error = "At least one rule needs a non-zero weight." });

        // Archive any other Published set with the same name — only one version of a given
        // named rule set is ever "the" active one at a time.
        var currentlyPublished = await db.MatchRuleSets
            .Where(s => s.Name == set.Name && s.Status == MatchRuleSetStatus.Published)
            .ToListAsync(ct);
        foreach (var old in currentlyPublished)
        {
            old.Status = MatchRuleSetStatus.Archived;
        }

        set.Status = MatchRuleSetStatus.Published;
        set.PublishedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var set = await db.MatchRuleSets.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (set is null) return NotFound();

        set.Status = MatchRuleSetStatus.Archived;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static MatchRuleSetDto ToDto(MatchRuleSet set) => new(
        set.Id, set.Name, set.Version, set.Status.ToString(), set.Description, set.CreatedAt, set.PublishedAt,
        set.Rules.OrderBy(r => r.SortOrder).Select(r => new MatchRuleDto(r.Id, r.Criterion, r.Weight, r.IsRequired, r.ToleranceValue, r.SortOrder)).ToList());
}
