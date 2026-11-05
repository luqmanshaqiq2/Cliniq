using Backend.Cliniq.Models.Entities;

//input records & operation results data between interfaces & services
namespace Cliniq.Services.DataServices.Contracts
{
    public sealed record AppointmentBookingResult(
        AppointmentBookingStatus Status,
        Appointment? Appointment = null);

    public enum AppointmentBookingStatus
    {
        Booked,
        SlotTaken,
        DoctorUnavailable,
        PatientOverlap,
        AppointmentNotFound,
        InvalidAppointmentState
    }
}
