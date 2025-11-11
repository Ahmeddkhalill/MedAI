using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddXraysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Xrays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    AI_Diagnosis = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AI_Confidence = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FinalDiagnosis = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    FinalConfidence = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsRevised = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PatientId = table.Column<string>(type: "TEXT", nullable: false),
                    DoctorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xrays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Xrays_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Xrays_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Xrays_DoctorId",
                table: "Xrays",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Xrays_PatientId",
                table: "Xrays",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Xrays");
        }
    }
}
