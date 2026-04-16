using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditRequestEnumsAsString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditRequests_Users_OwnerId",
                table: "EditRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "EditRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "EditRequests",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_EditRequests_Users_OwnerId",
                table: "EditRequests",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EditRequests_Users_OwnerId",
                table: "EditRequests");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "EditRequests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "EditRequests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_EditRequests_Users_OwnerId",
                table: "EditRequests",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
