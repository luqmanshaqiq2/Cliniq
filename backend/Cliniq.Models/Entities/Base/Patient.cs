using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Config;
using Microsoft.EntityFrameworkCore;

namespace Backend.Cliniq.Models.Entities.Base
{
    [EntityTypeConfiguration(typeof(PatientConfiguration))]
    public class Patient : BaseEntity
    {
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}