using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInsurerIdFromAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_insurers_InsurerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InsurerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InsurerId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "user_insurances",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    insurer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_insurances", x => new { x.user_id, x.insurer_id });
                    table.ForeignKey(
                        name: "FK_user_insurances_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_insurances_insurers_insurer_id",
                        column: x => x.insurer_id,
                        principalTable: "insurers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_insurances_insurer_id",
                table: "user_insurances",
                column: "insurer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_insurances");

            migrationBuilder.AddColumn<Guid>(
                name: "InsurerId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InsurerId",
                table: "AspNetUsers",
                column: "InsurerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_insurers_InsurerId",
                table: "AspNetUsers",
                column: "InsurerId",
                principalTable: "insurers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
