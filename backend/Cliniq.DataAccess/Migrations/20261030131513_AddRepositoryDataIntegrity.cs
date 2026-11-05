using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cliniq.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositoryDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_PositiveAmount",
                table: "Payments",
                sql: "CAST(\"Amount\" AS NUMERIC) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_PositiveAmount",
                table: "Invoices",
                sql: "CAST(\"Amount\" AS NUMERIC) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AvailabilitySlots_ValidSchedule",
                table: "AvailabilitySlots",
                sql: "\"StartAtUtc\" < \"EndAtUtc\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Appointments_ValidSchedule",
                table: "Appointments",
                sql: "\"ScheduledAtUtc\" < \"ScheduledEndAtUtc\"");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_PositiveAmount",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_PositiveAmount",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AvailabilitySlots_ValidSchedule",
                table: "AvailabilitySlots");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Appointments_ValidSchedule",
                table: "Appointments");
        }
    }
}
