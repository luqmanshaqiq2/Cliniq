using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(MedicalRecordConfiguration))]
    public class MedicalRecord : BaseEntity
    {
     public string Diagnosis { get; set; } = string.Empty;
        public string? Prescription { get; set; }
        public TreatmentType TreatmentType { get; set; }
        public string? SurgeryDetails { get; set; }
        public string? Notes { get; set; }

        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;

        public int? ApprovedByDoctorId { get; set; }
        public virtual Doctor? ApprovedByDoctor { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        public DateTime CreatedAt { get; set; }

    }

    public enum TreatmentType
    {
        Medication = 1,
        Surgery = 2,
        Therapy = 3,
        Procedure = 4,
        Other = 5
    }

}