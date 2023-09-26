using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasteBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddedUpvotesFunc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "upvotes",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "upvotes",
                table: "posts");
        }
    }
}
