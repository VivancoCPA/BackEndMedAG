using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FamiliyGroupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_family_memberships_user_id_family_group_id",
                table: "family_memberships");

            migrationBuilder.CreateIndex(
                name: "IX_family_memberships_user_id",
                table: "family_memberships",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_family_memberships_user_id",
                table: "family_memberships");

            migrationBuilder.CreateIndex(
                name: "IX_family_memberships_user_id_family_group_id",
                table: "family_memberships",
                columns: new[] { "user_id", "family_group_id" },
                unique: true);
        }
    }
}
