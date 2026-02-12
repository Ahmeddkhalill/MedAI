using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorNotesToXray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorNotes",
                table: "Xrays",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorNotes",
                table: "Xrays");
        }
    }
}
