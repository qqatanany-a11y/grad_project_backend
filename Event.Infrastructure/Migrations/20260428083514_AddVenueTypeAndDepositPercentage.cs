using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueTypeAndDepositPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositPercentage",
                table: "Venues",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Venues",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositPercentage",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Venues");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
