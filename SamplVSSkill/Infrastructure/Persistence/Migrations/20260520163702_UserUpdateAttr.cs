using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserUpdateAttr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_insurers_insurer_id",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetUsers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "insurer_id",
                table: "AspNetUsers",
                newName: "InsurerId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_insurer_id",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_InsurerId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_insurers_InsurerId",
                table: "AspNetUsers",
                column: "InsurerId",
                principalTable: "insurers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_insurers_InsurerId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "InsurerId",
                table: "AspNetUsers",
                newName: "insurer_id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_InsurerId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_insurer_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_insurers_insurer_id",
                table: "AspNetUsers",
                column: "insurer_id",
                principalTable: "insurers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
