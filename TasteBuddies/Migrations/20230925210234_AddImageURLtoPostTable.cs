using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasteBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddImageURLtoPostTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "posts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "posts");
        }
    }
}
