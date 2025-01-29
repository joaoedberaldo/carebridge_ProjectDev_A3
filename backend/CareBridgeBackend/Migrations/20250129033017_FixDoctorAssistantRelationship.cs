using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareBridgeBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixDoctorAssistantRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAssistants_Users_UserId",
                table: "DoctorAssistants");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAssistants_Users_UserId1",
                table: "DoctorAssistants");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAssistants_UserId",
                table: "DoctorAssistants");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAssistants_UserId1",
                table: "DoctorAssistants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DoctorAssistants");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "DoctorAssistants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DoctorAssistants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "DoctorAssistants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAssistants_UserId",
                table: "DoctorAssistants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAssistants_UserId1",
                table: "DoctorAssistants",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAssistants_Users_UserId",
                table: "DoctorAssistants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAssistants_Users_UserId1",
                table: "DoctorAssistants",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
