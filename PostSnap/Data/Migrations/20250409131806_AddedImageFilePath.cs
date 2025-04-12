using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostSnap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Posts");
        }
    }
}
