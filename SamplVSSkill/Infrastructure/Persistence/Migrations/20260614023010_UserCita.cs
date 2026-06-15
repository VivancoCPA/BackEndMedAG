using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamplVSSkill.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointment_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    center_id = table.Column<Guid>(type: "uuid", nullable: true),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    specialtie_id = table.Column<int>(type: "integer", nullable: true),
                    insurer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointment_users_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_appointment_users_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_users_insurers_insurer_id",
                        column: x => x.insurer_id,
                        principalTable: "insurers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_users_medical_centers_center_id",
                        column: x => x.center_id,
                        principalTable: "medical_centers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_users_specialties_specialtie_id",
                        column: x => x.specialtie_id,
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_users_center_id",
                table: "appointment_users",
                column: "center_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_users_doctor_id",
                table: "appointment_users",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_users_insurer_id",
                table: "appointment_users",
                column: "insurer_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_users_specialtie_id",
                table: "appointment_users",
                column: "specialtie_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_users_user_id",
                table: "appointment_users",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_users");
        }
    }
}
