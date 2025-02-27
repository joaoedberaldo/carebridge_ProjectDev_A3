using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public User Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public int? AppointmentId { get; set; }
        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; }

        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

        public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    }

    public enum PrescriptionStatus
    {
        Active,
        Completed,
        Canceled
    }
}
