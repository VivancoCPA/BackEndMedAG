using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Catalogosv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "specialty",
                table: "doctors");

            migrationBuilder.RenameColumn(
                name: "Register",
                table: "doctors",
                newName: "register");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "doctors",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "doctors",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "doctors",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PhotoUrl",
                table: "doctors",
                newName: "photo_url");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "doctors",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "doctors",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "doctors",
                newName: "created_at");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "doctors",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "specialty_id",
                table: "doctors",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "insurers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    person_in_charge = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insurers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_specialties", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doctors_specialty_id",
                table: "doctors",
                column: "specialty_id");

            migrationBuilder.AddForeignKey(
                name: "FK_doctors_specialties_specialty_id",
                table: "doctors",
                column: "specialty_id",
                principalTable: "specialties",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctors_specialties_specialty_id",
                table: "doctors");

            migrationBuilder.DropTable(
                name: "insurers");

            migrationBuilder.DropTable(
                name: "specialties");

            migrationBuilder.DropIndex(
                name: "IX_doctors_specialty_id",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "specialty_id",
                table: "doctors");

            migrationBuilder.RenameColumn(
                name: "register",
                table: "doctors",
                newName: "Register");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "doctors",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "doctors",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "doctors",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "photo_url",
                table: "doctors",
                newName: "PhotoUrl");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "doctors",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "doctors",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "doctors",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "doctors",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "specialty",
                table: "doctors",
                type: "text",
                nullable: true);
        }
    }
}
