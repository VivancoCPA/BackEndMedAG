using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MedicalCenter_TypeFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "medical_centers");

            migrationBuilder.AddColumn<int>(
                name: "type_id",
                table: "medical_centers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_centers_type_id",
                table: "medical_centers",
                column: "type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_medical_centers_centers_type_type_id",
                table: "medical_centers",
                column: "type_id",
                principalTable: "centers_type",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_medical_centers_centers_type_type_id",
                table: "medical_centers");

            migrationBuilder.DropIndex(
                name: "IX_medical_centers_type_id",
                table: "medical_centers");

            migrationBuilder.DropColumn(
                name: "type_id",
                table: "medical_centers");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "medical_centers",
                type: "text",
                nullable: true);
        }
    }
}
