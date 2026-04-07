using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MiddleName", "PasswordHash" },
                values: new object[] { "Naser", "$2b$11$wH8s6xHh9v7Zy0Q0v3P7uO9R9vT7kJrZz6lHqRzYwzKQ1x9kWQh7K" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MiddleName", "PasswordHash" },
                values: new object[] { "", "AQAAAAIAAYagAAAAEFcpGGxDf6ABeWoCUO18X4+XansM1SITeg0RmUlQfJ26wIfeNveydnOnYZMs9tghLQ==" });
        }
    }
}
