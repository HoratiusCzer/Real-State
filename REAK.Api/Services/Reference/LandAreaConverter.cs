using REAK.Api.Models.Enums;

namespace REAK.Api.Services.Reference;

/// <summary>Nepal land-area unit conversion (spec §11). These coefficients are standard,
/// officially-recognized values, explicitly approved for use here — satisfying spec §11's
/// "conversion stays disabled unless explicitly configured and approved" condition. Hardcoded
/// rather than read from AreaUnit.ConversionToSquareMeters: a compound entry (e.g. "4 Ropani 2
/// Paisa") is a weighted sum across multiple AreaUnit rows at once, which a single per-row DB
/// coefficient can't express, and these values are fixed geography-based constants, not
/// admin-editable business data.</summary>
public static class LandAreaConverter
{
    public const decimal RopaniToSquareFeet = 5476m;
    public const decimal AanaToSquareFeet = 342.25m;
    public const decimal PaisaToSquareFeet = 85.5625m;
    public const decimal DamToSquareFeet = 21.390625m;

    public const decimal BighaToSquareFeet = 72900m;
    public const decimal KatthaToSquareFeet = 3645m;
    public const decimal DhurToSquareFeet = 182.25m;

    public const decimal SquareMetresToSquareFeet = 10.7639m;

    public const int AanaMax = 15;
    public const int PaisaMax = 3;
    public const int DamMax = 3;
    public const int KatthaMax = 19;
    public const int DhurMax = 19;

    /// <summary>Per-unit factor keyed by the exact AreaUnit.Name strings DatabaseSeeder uses —
    /// for converting a single already-stored decimal + unit (legacy listings, and Demand's
    /// MinArea/MaxArea, which stay single-value until the demand side of this feature is built)
    /// into square feet on the fly, without needing that row's own compound entry.</summary>
    public static readonly IReadOnlyDictionary<string, decimal> SquareFeetPerUnitName = new Dictionary<string, decimal>
    {
        ["Ropani"] = RopaniToSquareFeet,
        ["Aana"] = AanaToSquareFeet,
        ["Paisa"] = PaisaToSquareFeet,
        ["Dam"] = DamToSquareFeet,
        ["Bigha"] = BighaToSquareFeet,
        ["Kattha"] = KatthaToSquareFeet,
        ["Dhur"] = DhurToSquareFeet,
        ["Square Feet"] = 1m,
        ["Square Metres"] = SquareMetresToSquareFeet,
    };

    /// <summary>Converts a single value + unit name to square feet, or null if the unit name
    /// isn't recognized (shouldn't happen for a real AreaUnit row, but a demand's AreaUnitId can
    /// be null, and callers pass through whatever AreaUnit.Name? they have).</summary>
    public static decimal? ToSquareFeet(decimal value, string? unitName) =>
        unitName is not null && SquareFeetPerUnitName.TryGetValue(unitName, out var factor) ? value * factor : null;

    public static decimal RopaniCompoundToSquareFeet(int ropani, int aana, int paisa, int dam) =>
        ropani * RopaniToSquareFeet + aana * AanaToSquareFeet + paisa * PaisaToSquareFeet + dam * DamToSquareFeet;

    public static decimal BighaCompoundToSquareFeet(int bigha, int kattha, int dhur) =>
        bigha * BighaToSquareFeet + kattha * KatthaToSquareFeet + dhur * DhurToSquareFeet;

    /// <summary>The legacy single-value LandArea a compound entry corresponds to, expressed in
    /// its own system's largest unit (e.g. "4 Ropani 0 Aana 2 Paisa" -> ~4.03125 Ropani) — kept
    /// for backward compatibility with code that still reads LandArea/AreaUnitId directly. The
    /// exact original integers live in the compound columns; this is a derived convenience, never
    /// the source of truth.</summary>
    public static decimal RopaniCompoundToRopaniDecimal(int ropani, int aana, int paisa, int dam) =>
        ropani + aana / 16m + paisa / 64m + dam / 256m;

    public static decimal BighaCompoundToBighaDecimal(int bigha, int kattha, int dhur) =>
        bigha + kattha / 20m + dhur / 400m;
}
