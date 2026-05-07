using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionColumnToDocotrsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Doctors",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Doctors");
        }
    }
}
