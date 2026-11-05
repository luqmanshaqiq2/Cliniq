using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class PatientDataService
    : BaseDataService.BaseDalDataService<Patient, PatientDataService>,
      IPatientDataService
{
    public PatientDataService(IPatientRepo repository, IAppLogging<PatientDataService> logger)
        : base(repository, logger)
    {
    }

    public async Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
        string name,
        DateTime dateOfBirth,
        string phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        var normalizedName = name.Trim().ToLower();
        var normalizedPhone = phone.Trim();

        return await _mainRepo.Context.Patients
            .AsNoTracking()
            .Where(patient =>
                patient.DateOfBirth == dateOfBirth &&
                patient.FullName.ToLower() == normalizedName &&
                patient.ContactNumber == normalizedPhone)
            .OrderBy(patient => patient.Id)
            .ToListAsync();
    }

    public Task<bool> ExistsByNationalIdAsync(string nationalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nationalId);
        throw new NotSupportedException(
            "National-ID checks are unavailable because the patient model has no national-ID field.");
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.Trim().ToLower();
        return await _mainRepo.Context.Patients.AnyAsync(patient =>
            patient.Email != null && patient.Email.ToLower() == normalizedEmail);
    }

    public async Task<Patient?> MergeAsync(int primaryId, int duplicateId)
    {
        if (primaryId == duplicateId)
        {
            throw new ArgumentException("Primary and duplicate patient IDs must differ.");
        }

        var context = _mainRepo.Context;
        var primary = await context.Patients.SingleOrDefaultAsync(patient => patient.Id == primaryId);
        var duplicate = await context.Patients.SingleOrDefaultAsync(patient => patient.Id == duplicateId);
        if (primary is null || duplicate is null)
        {
            return null;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        var appointments = await context.Appointments
            .Where(appointment => appointment.PatientId == duplicateId)
            .ToListAsync();
        var medicalRecords = await context.MedicalRecords
            .Where(record => record.PatientId == duplicateId)
            .ToListAsync();
        var invoices = await context.Invoices
            .Where(invoice => invoice.PatientId == duplicateId)
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            appointment.PatientId = primaryId;
        }

        foreach (var record in medicalRecords)
        {
            record.PatientId = primaryId;
        }

        foreach (var invoice in invoices)
        {
            invoice.PatientId = primaryId;
        }

        context.Patients.Remove(duplicate);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return primary;
    }

    public async Task<Patient?> GetWithOutstandingBalanceAsync(int patientId)
    {
        return await _mainRepo.Context.Patients
            .Include(patient => patient.Invoices)
            .ThenInclude(invoice => invoice.Payments)
            .SingleOrDefaultAsync(patient =>
                patient.Id == patientId &&
                patient.Invoices.Any(invoice =>
                    invoice.Status != InvoiceStatus.Void &&
                    invoice.Amount >
                    (invoice.Payments
                        .Where(payment => payment.Status == PaymentStatus.Completed)
                        .Sum(payment => (decimal?)payment.Amount) ?? 0m)));
    }

    public async Task<IReadOnlyList<Patient>> SearchAsync(string term, int pageSize, int pageNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(term);
        ValidatePage(pageSize, pageNumber);
        var pattern = $"%{term.Trim()}%";

        return await _mainRepo.Context.Patients
            .AsNoTracking()
            .Where(patient =>
                EF.Functions.Like(patient.FullName, pattern) ||
                EF.Functions.Like(patient.ContactNumber, pattern) ||
                (patient.Email != null && EF.Functions.Like(patient.Email, pattern)))
            .OrderBy(patient => patient.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> AnonymizeAsync(int patientId)
    {
        var patient = await _mainRepo.Context.Patients
            .SingleOrDefaultAsync(item => item.Id == patientId);
        if (patient is null)
        {
            return false;
        }

        patient.FullName = $"Anonymized patient {patient.Id}";
        patient.DateOfBirth = DateTime.UnixEpoch;
        patient.ContactNumber = "REDACTED";
        patient.Email = $"anonymized-{patient.Id}@invalid.local";
        patient.Address = null;
        await _mainRepo.Context.SaveChangesAsync();
        return true;
    }

    private static void ValidatePage(int pageSize, int pageNumber)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }
    }
}
