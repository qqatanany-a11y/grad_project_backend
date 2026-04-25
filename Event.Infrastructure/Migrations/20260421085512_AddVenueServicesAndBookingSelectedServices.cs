using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Event.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueServicesAndBookingSelectedServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VenueAvailabilities_Companies_CompanyId",
                table: "VenueAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Companies_CompanyId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_VenueAvailabilities_CompanyId",
                table: "VenueAvailabilities");

            migrationBuilder.DropColumn(
                name: "MinimalPrice",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "VenueAvailabilities");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "VenueAvailabilities");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Venues",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingType",
                table: "Venues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"ALTER TABLE ""VenueAvailabilities"" 
                  ALTER COLUMN ""StartTime"" TYPE interval 
                  USING (""StartTime""::time - time '00:00:00');");

            migrationBuilder.Sql(
                @"ALTER TABLE ""VenueAvailabilities"" 
                  ALTER COLUMN ""EndTime"" TYPE interval 
                  USING CASE 
                      WHEN ""EndTime"" IS NULL THEN interval '00:00:00'
                      ELSE (""EndTime""::time - time '00:00:00')
                  END;");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "VenueAvailabilities",
                type: "interval",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "VenueAvailabilities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "VenueAvailabilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "VenueAvailabilities",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Bookings",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VenueServiceOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueServiceOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueServiceOptions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VenueServiceOptions_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingSelectedServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    VenueServiceOptionId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSelectedServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSelectedServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingSelectedServices_VenueServiceOptions_VenueServiceOpt~",
                        column: x => x.VenueServiceOptionId,
                        principalTable: "VenueServiceOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingSelectedServices_BookingId",
                table: "BookingSelectedServices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSelectedServices_VenueServiceOptionId",
                table: "BookingSelectedServices",
                column: "VenueServiceOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueServiceOptions_ServiceId",
                table: "VenueServiceOptions",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueServiceOptions_VenueId_ServiceId",
                table: "VenueServiceOptions",
                columns: new[] { "VenueId", "ServiceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Companies_CompanyId",
                table: "Venues",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Companies_CompanyId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "BookingSelectedServices");

            migrationBuilder.DropTable(
                name: "VenueServiceOptions");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "PricingType",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "VenueAvailabilities");

            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "VenueAvailabilities");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "VenueAvailabilities");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimalPrice",
                table: "Venues",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                @"ALTER TABLE ""VenueAvailabilities"" 
                  ALTER COLUMN ""StartTime"" TYPE timestamp with time zone 
                  USING (date '2000-01-01' + ""StartTime"");");

            migrationBuilder.Sql(
                @"ALTER TABLE ""VenueAvailabilities"" 
                  ALTER COLUMN ""EndTime"" TYPE timestamp with time zone 
                  USING (date '2000-01-01' + ""EndTime"");");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "VenueAvailabilities",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "VenueAvailabilities",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "VenueAvailabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "VenueAvailabilities",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Bookings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "IX_VenueAvailabilities_CompanyId",
                table: "VenueAvailabilities",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_VenueAvailabilities_Companies_CompanyId",
                table: "VenueAvailabilities",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Companies_CompanyId",
                table: "Venues",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}