using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_of_birth",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "insurer_id",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_insurer_id",
                table: "AspNetUsers",
                column: "insurer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_insurers_insurer_id",
                table: "AspNetUsers",
                column: "insurer_id",
                principalTable: "insurers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_insurers_insurer_id",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_insurer_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "date_of_birth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "insurer_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "name",
                table: "AspNetUsers");
        }
    }
}
