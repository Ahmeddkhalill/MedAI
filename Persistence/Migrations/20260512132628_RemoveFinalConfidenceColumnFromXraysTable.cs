using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFinalConfidenceColumnFromXraysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalConfidence",
                table: "Xrays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalConfidence",
                table: "Xrays",
                type: "decimal(5,2)",
                nullable: true);
        }
    }
}
