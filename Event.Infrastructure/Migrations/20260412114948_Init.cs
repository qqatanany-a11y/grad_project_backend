using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "MiddleName", "PasswordHash", "PhoneNumber", "RoleId", "SecondaryPhoneNumber", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "omar@gmail.com", "Omar", true, "Admin", "Naser", "$2b$11$wH8s6xHh9v7Zy0Q0v3P7uO9R9vT7kJrZz6lHqRzYwzKQ1x9kWQh7K", "0796096783", 1, null, null });
        }
    }
}
