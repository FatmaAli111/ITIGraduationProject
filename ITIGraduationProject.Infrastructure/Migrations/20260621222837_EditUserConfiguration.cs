using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITIGraduationProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditUserConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedByAdminId",
                table: "GraphicAssets",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GraphicAssets_UserId",
                table: "GraphicAssets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GraphicAssets_Users_UserId",
                table: "GraphicAssets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GraphicAssets_Users_UserId",
                table: "GraphicAssets");

            migrationBuilder.DropIndex(
                name: "IX_GraphicAssets_UserId",
                table: "GraphicAssets");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "GraphicAssets",
                newName: "UploadedByAdminId");
        }
    }
}
