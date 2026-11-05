using Backend.Cliniq.Models.Entities;
using Backend.Cliniq.Models.Entities.Base;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Cliniq.DataAccess.EFStructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<SerilogEntry> SerilogEntries => Set<SerilogEntry>();
        public DbSet<User> Users => Set<User>();

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Entity specific configurations
            new AppointmentConfiguration().Configure(modelBuilder.Entity<Appointment>());
            new AvailabilitySlotConfiguration().Configure(modelBuilder.Entity<AvailabilitySlot>());
            new DoctorConfiguration().Configure(modelBuilder.Entity<Doctor>());
            new InvoiceConfiguration().Configure(modelBuilder.Entity<Invoice>());
            new MedicalRecordConfiguration().Configure(modelBuilder.Entity<MedicalRecord>());
            new PatientConfiguration().Configure(modelBuilder.Entity<Patient>());
            new PaymentConfiguration().Configure(modelBuilder.Entity<Payment>());
            new SerilogEntryConfiguration().Configure(modelBuilder.Entity<SerilogEntry>());
            new UserConfiguration().Configure(modelBuilder.Entity<User>());

            if (Database.IsSqlite())
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                             .Where(entityType => typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.TimeStamp))
                        .ValueGeneratedNever();
                }
            }
        }
    }

    
}