using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddbooleanIsAccountCompletedToDoctorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccountCompleted",
                table: "Doctors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccountCompleted",
                table: "Doctors");
        }
    }
}
