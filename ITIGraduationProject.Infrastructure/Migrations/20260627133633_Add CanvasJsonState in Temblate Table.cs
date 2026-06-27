using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITIGraduationProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCanvasJsonStateinTemblateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanvasStateJSON",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanvasStateJSON",
                table: "Templates");
        }
    }
}
