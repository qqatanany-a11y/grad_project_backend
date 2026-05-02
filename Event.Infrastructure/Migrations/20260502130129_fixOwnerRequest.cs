using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixOwnerRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VenueName",
                table: "OwnerRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                table: "OwnerRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
