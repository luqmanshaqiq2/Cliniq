using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;

namespace Cliniq.DataAccess.Repos
{
    public class PatientRepo : BaseRepo<Patient>, IPatientRepo
    {
        public PatientRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
