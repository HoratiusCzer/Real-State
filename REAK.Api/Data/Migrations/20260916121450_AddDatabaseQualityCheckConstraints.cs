using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REAK.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseQualityCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_Bathrooms",
                table: "PropertyListings",
                sql: "[Bathrooms] IS NULL OR [Bathrooms] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_Bedrooms",
                table: "PropertyListings",
                sql: "[Bedrooms] IS NULL OR [Bedrooms] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_BuiltUpArea",
                table: "PropertyListings",
                sql: "[BuiltUpArea] IS NULL OR [BuiltUpArea] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_Floors",
                table: "PropertyListings",
                sql: "[Floors] IS NULL OR [Floors] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_LandArea",
                table: "PropertyListings",
                sql: "[LandArea] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_ParkingSpaces",
                table: "PropertyListings",
                sql: "[ParkingSpaces] IS NULL OR [ParkingSpaces] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PropertyListings_Price",
                table: "PropertyListings",
                sql: "[Price] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MatchRules_Weight",
                table: "MatchRules",
                sql: "[Weight] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Matches_Score",
                table: "Matches",
                sql: "[Score] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_AreaRange",
                table: "Demands",
                sql: "[MinArea] IS NULL OR [MaxArea] IS NULL OR [MaxArea] >= [MinArea]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_BudgetRange",
                table: "Demands",
                sql: "[MinBudget] IS NULL OR [MaxBudget] IS NULL OR [MaxBudget] >= [MinBudget]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MaxArea",
                table: "Demands",
                sql: "[MaxArea] IS NULL OR [MaxArea] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MaxBudget",
                table: "Demands",
                sql: "[MaxBudget] IS NULL OR [MaxBudget] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MinArea",
                table: "Demands",
                sql: "[MinArea] IS NULL OR [MinArea] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MinBathrooms",
                table: "Demands",
                sql: "[MinBathrooms] IS NULL OR [MinBathrooms] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MinBedrooms",
                table: "Demands",
                sql: "[MinBedrooms] IS NULL OR [MinBedrooms] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Demands_MinBudget",
                table: "Demands",
                sql: "[MinBudget] IS NULL OR [MinBudget] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_Bathrooms",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_Bedrooms",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_BuiltUpArea",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_Floors",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_LandArea",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_ParkingSpaces",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PropertyListings_Price",
                table: "PropertyListings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MatchRules_Weight",
                table: "MatchRules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Matches_Score",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_AreaRange",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_BudgetRange",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MaxArea",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MaxBudget",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MinArea",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MinBathrooms",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MinBedrooms",
                table: "Demands");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Demands_MinBudget",
                table: "Demands");
        }
    }
}
