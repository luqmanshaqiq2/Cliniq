using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using Cliniq.Services.DataServices.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IMedicalRecordDataService : IBaseDataService<MedicalRecord>
    {
        Task<MedicalRecord> AddAmendmentAsync(
            int recordId,
            MedicalRecordAmendment amendment);

        Task<IReadOnlyList<MedicalRecord>> GetVersionHistoryAsync(int recordId);

        Task<IReadOnlyList<MedicalRecord>> GetByPatientAsync(
            int patientId,
            int requesterDoctorId);

        Task<bool> ExistsForAppointmentAsync(int appointmentId);

        Task<bool> LockAsync(int recordId);

        Task<IReadOnlyList<MedicalRecord>> GetUnfinalizedOlderThanAsync(TimeSpan span);
    }
}
