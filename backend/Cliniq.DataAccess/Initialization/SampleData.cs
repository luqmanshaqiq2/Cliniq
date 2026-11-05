using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;

namespace Cliniq.DataAccess.Initialization;

public static class SampleData
{
    public static (IReadOnlyList<Doctor> Doctors, IReadOnlyList<Patient> Patients) Create()
    {
        var now = DateTime.UtcNow;
        var doctor = new Doctor
        {
            FullName = "Alex Morgan",
            Specialization = "Family Medicine",
            ContactNumber = "+1-555-0100",
            Email = "alex.morgan@example.test",
            CreatedAt = now
        };

        var patient = new Patient
        {
            FullName = "Jordan Lee",
            DateOfBirth = new DateTime(1990, 5, 12, 0, 0, 0, DateTimeKind.Utc),
            ContactNumber = "+1-555-0101",
            Email = "jordan.lee@example.test",
            Address = "100 Example Street",
            CreatedAt = now
        };

        var secondPatient = new Patient
        {
            FullName = "Taylor Rivera",
            DateOfBirth = new DateTime(1985, 9, 23, 0, 0, 0, DateTimeKind.Utc),
            ContactNumber = "+1-555-0102",
            Email = "taylor.rivera@example.test",
            Address = "200 Example Avenue",
            CreatedAt = now
        };

        var completedAppointment = new Appointment
        {
            ScheduledAtUtc = now.AddDays(-7),
            ScheduledEndAtUtc = now.AddDays(-7).AddMinutes(30),
            Notes = "Routine check-up",
            Status = AppointmentStatus.Completed,
            CreatedAt = now.AddDays(-14),
            Patient = patient,
            Doctor = doctor
        };
        patient.Appointments.Add(completedAppointment);
        doctor.Appointments.Add(completedAppointment);

        var medicalRecord = new MedicalRecord
        {
            Diagnosis = "Seasonal allergies",
            Prescription = "Over-the-counter antihistamine as directed",
            TreatmentType = TreatmentType.Medication,
            Notes = "Follow up if symptoms persist.",
            CreatedAt = now.AddDays(-7),
            Patient = patient,
            Appointment = completedAppointment,
            ApprovedByDoctor = doctor
        };
        completedAppointment.MedicalRecord = medicalRecord;
        patient.MedicalRecords.Add(medicalRecord);

        var invoice = new Invoice
        {
            InvoiceNumber = "SAMPLE-INV-001",
            Amount = 75.00m,
            IssuedAtUtc = now.AddDays(-7),
            Status = InvoiceStatus.Paid,
            Patient = patient,
            Appointment = completedAppointment
        };
        var payment = new Payment
        {
            Amount = invoice.Amount,
            PaidAtUtc = now.AddDays(-7),
            Method = PaymentMethod.Card,
            Status = PaymentStatus.Completed,
            TransactionReference = "SAMPLE-TXN-001",
            Invoice = invoice
        };
        invoice.Payments.Add(payment);
        patient.Invoices.Add(invoice);

        var upcomingAppointment = new Appointment
        {
            ScheduledAtUtc = now.Date.AddDays(1).AddHours(10),
            ScheduledEndAtUtc = now.Date.AddDays(1).AddHours(10).AddMinutes(30),
            Notes = "Follow-up consultation",
            Status = AppointmentStatus.Confirmed,
            CreatedAt = now,
            Patient = secondPatient,
            Doctor = doctor
        };
        secondPatient.Appointments.Add(upcomingAppointment);
        doctor.Appointments.Add(upcomingAppointment);

        var availabilitySlot = new AvailabilitySlot
        {
            StartAtUtc = now.Date.AddDays(1).AddHours(9),
            EndAtUtc = now.Date.AddDays(1).AddHours(17),
            IsAvailable = true,
            Doctor = doctor
        };
        doctor.AvailabilitySlots.Add(availabilitySlot);

        BaseEntity[] entities =
        [
            doctor,
            patient,
            secondPatient,
            completedAppointment,
            medicalRecord,
            invoice,
            payment,
            upcomingAppointment,
            availabilitySlot
        ];
        foreach (var entity in entities)
        {
            entity.TimeStamp = [0];
        }

        return (new[] { doctor }, new[] { patient, secondPatient });
    }
}
