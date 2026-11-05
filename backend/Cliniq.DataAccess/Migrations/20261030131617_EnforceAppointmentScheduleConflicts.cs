using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cliniq.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAppointmentScheduleConflicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_Appointments_NoOverlap_Insert
                BEFORE INSERT ON Appointments
                FOR EACH ROW
                WHEN NEW.Status <> 'Cancelled'
                  AND EXISTS (
                    SELECT 1
                    FROM Appointments existing
                    WHERE existing.Status <> 'Cancelled'
                      AND (existing.DoctorId = NEW.DoctorId OR existing.PatientId = NEW.PatientId)
                      AND existing.ScheduledAtUtc < NEW.ScheduledEndAtUtc
                      AND NEW.ScheduledAtUtc < existing.ScheduledEndAtUtc
                  )
                BEGIN
                    SELECT RAISE(ABORT, 'Appointment overlaps an existing active appointment');
                END;

                CREATE TRIGGER TR_Appointments_NoOverlap_Update
                BEFORE UPDATE OF DoctorId, PatientId, ScheduledAtUtc, ScheduledEndAtUtc, Status
                ON Appointments
                FOR EACH ROW
                WHEN NEW.Status <> 'Cancelled'
                  AND EXISTS (
                    SELECT 1
                    FROM Appointments existing
                    WHERE existing.Id <> NEW.Id
                      AND existing.Status <> 'Cancelled'
                      AND (existing.DoctorId = NEW.DoctorId OR existing.PatientId = NEW.PatientId)
                      AND existing.ScheduledAtUtc < NEW.ScheduledEndAtUtc
                      AND NEW.ScheduledAtUtc < existing.ScheduledEndAtUtc
                  )
                BEGIN
                    SELECT RAISE(ABORT, 'Appointment overlaps an existing active appointment');
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS TR_Appointments_NoOverlap_Update;
                DROP TRIGGER IF EXISTS TR_Appointments_NoOverlap_Insert;
                """);
        }
    }
}
