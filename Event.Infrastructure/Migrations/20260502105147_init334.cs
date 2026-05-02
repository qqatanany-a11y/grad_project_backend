using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init334 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "Venues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "Venues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Venues",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Venues");
        }
    }
}
