using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "doctors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "doctors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "doctors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Register",
                table: "doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "doctors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "Register",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "doctors");
        }
    }
}
