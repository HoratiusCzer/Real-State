using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REAK.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompoundLandAreaToListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AanaValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AreaInSquareFeet",
                table: "PropertyListings",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BighaValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DhurValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KatthaValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeasurementSystem",
                table: "PropertyListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaisaValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RopaniValue",
                table: "PropertyListings",
                type: "int",
                nullable: true);

            // Backfill for rows that existed before this migration: AreaInSquareFeet is computed
            // exactly from the existing LandArea + AreaUnit (no precision loss — plain decimal
            // arithmetic). MeasurementSystem is a best-effort classification from the existing
            // single AreaUnit; the compound integer columns (RopaniValue/AanaValue/etc.) are left
            // null, since pre-migration data was never entered as a compound split — there's
            // nothing accurate to backfill them with, and a fractional legacy value (e.g. "4.5
            // Aana") isn't representable in the new whole-number-per-sub-unit model anyway.
            // Re-opening a pre-migration listing to edit will show blank compound fields; LandArea
            // itself stays intact and correct throughout.
            // PropertyListings has RLS (Data/Security/RowLevelSecurity.sql); a connection with no
            // app.is_system_admin session context set — which a migration's connection is — gets
            // silently filtered to 0 affected rows here, not an error. Bypass for this one
            // backfill statement the same way the app's own SessionContextOverride does for a
            // system-level computation.
            migrationBuilder.Sql("EXEC sp_set_session_context @key = N'app.is_system_admin', @value = 1;");
            migrationBuilder.Sql(@"
                UPDATE pl
                SET
                    AreaInSquareFeet = pl.LandArea * CASE au.Name
                        WHEN 'Ropani' THEN 5476
                        WHEN 'Aana' THEN 342.25
                        WHEN 'Paisa' THEN 85.5625
                        WHEN 'Dam' THEN 21.390625
                        WHEN 'Bigha' THEN 72900
                        WHEN 'Kattha' THEN 3645
                        WHEN 'Dhur' THEN 182.25
                        WHEN 'Square Feet' THEN 1
                        WHEN 'Square Metres' THEN 10.7639
                        ELSE 1
                    END,
                    MeasurementSystem = CASE au.Name
                        WHEN 'Ropani' THEN 1
                        WHEN 'Aana' THEN 1
                        WHEN 'Paisa' THEN 1
                        WHEN 'Dam' THEN 1
                        WHEN 'Bigha' THEN 2
                        WHEN 'Kattha' THEN 2
                        WHEN 'Dhur' THEN 2
                        WHEN 'Square Feet' THEN 3
                        WHEN 'Square Metres' THEN 4
                        ELSE 3
                    END
                FROM PropertyListings pl
                JOIN AreaUnits au ON au.Id = pl.AreaUnitId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AanaValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "AreaInSquareFeet",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "BighaValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "DamValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "DhurValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "KatthaValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "MeasurementSystem",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "PaisaValue",
                table: "PropertyListings");

            migrationBuilder.DropColumn(
                name: "RopaniValue",
                table: "PropertyListings");
        }
    }
}
