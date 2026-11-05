using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Services.DataServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cliniq.Services.DataServices.Interfaces
{
    public interface IPatientDataService : IBaseDataService<Patient>
    {
        Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
            string name,
            DateTime dateOfBirth,
            string phone);

        Task<bool> ExistsByNationalIdAsync(string nationalId);

        Task<bool> ExistsByEmailAsync(string email);

        Task<Patient?> MergeAsync(int primaryId, int duplicateId);

        Task<Patient?> GetWithOutstandingBalanceAsync(int patientId);

        Task<IReadOnlyList<Patient>> SearchAsync(
            string term,
            int pageSize,
            int pageNumber);

        Task<bool> AnonymizeAsync(int patientId);
    }
}
