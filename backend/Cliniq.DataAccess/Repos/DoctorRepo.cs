using Backend.Cliniq.Models.Entities.Base;
using Cliniq.DataAccess.EFStructure;
using Cliniq.DataAccess.Repos.Base;
using Cliniq.DataAccess.Repos.interfaces;

namespace Cliniq.DataAccess.Repos
{
    public class DoctorRepo : BaseRepo<Doctor>, IDoctorRepo
    {
        public DoctorRepo(ApplicationDbContext context) : base(context)
        {
        }
    }
}
