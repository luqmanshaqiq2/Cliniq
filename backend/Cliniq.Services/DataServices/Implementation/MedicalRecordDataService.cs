using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using Cliniq.Services.DataServices.Interfaces;
using Cliniq.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.Services.DataServices.Implementation;

public sealed class MedicalRecordDataService
    : BaseDataService.BaseDalDataService<MedicalRecord, MedicalRecordDataService>,
      IMedicalRecordDataService
{
    public MedicalRecordDataService(
        IMedicalRecordRepo repository,
        IAppLogging<MedicalRecordDataService> logger)
        : base(repository, logger)
    {
    }

    public Task<MedicalRecord> AddAmendmentAsync(int recordId, MedicalRecordAmendment amendment)
    {
        ArgumentNullException.ThrowIfNull(amendment);
        ArgumentException.ThrowIfNullOrWhiteSpace(amendment.Reason);
        throw new NotSupportedException(
            "Medical-record amendments cannot preserve version history because the current model has no version or amendment entity.");
    }

    public Task<IReadOnlyList<MedicalRecord>> GetVersionHistoryAsync(int recordId)
    {
        throw new NotSupportedException(
            "Medical-record version history is unavailable because the current model has no version or amendment entity.");
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetByPatientAsync(
        int patientId,
        int requesterDoctorId)
    {
        return await _mainRepo.Context.MedicalRecords
            .AsNoTracking()
            .Where(record =>
                record.PatientId == patientId &&
                (record.ApprovedByDoctorId == requesterDoctorId ||
                 record.Appointment.DoctorId == requesterDoctorId))
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync();
    }

    public Task<bool> ExistsForAppointmentAsync(int appointmentId)
    {
        return _mainRepo.Context.MedicalRecords
            .AnyAsync(record => record.AppointmentId == appointmentId);
    }

    public Task<bool> LockAsync(int recordId)
    {
        throw new NotSupportedException(
            "Medical-record locking cannot be persisted because the current model has no lock state.");
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetUnfinalizedOlderThanAsync(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(span));
        }

        var cutoff = DateTime.UtcNow - span;
        return await _mainRepo.Context.MedicalRecords
            .AsNoTracking()
            .Where(record => record.ApprovedAtUtc == null && record.CreatedAt < cutoff)
            .OrderBy(record => record.CreatedAt)
            .ToListAsync();
    }
}
