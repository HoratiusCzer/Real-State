using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REAK.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceCodeSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE dbo.ListingReferenceCodeSeq AS INT START WITH 1 INCREMENT BY 1;");
            migrationBuilder.Sql("CREATE SEQUENCE dbo.DemandReferenceCodeSeq AS INT START WITH 1 INCREMENT BY 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE dbo.ListingReferenceCodeSeq;");
            migrationBuilder.Sql("DROP SEQUENCE dbo.DemandReferenceCodeSeq;");
        }
    }
}
