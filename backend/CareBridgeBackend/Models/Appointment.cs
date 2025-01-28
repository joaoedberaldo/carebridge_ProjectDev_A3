using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public int DoctorId { get; set; } 
        public Doctor Doctor { get; set; } 

        [Required]
        public int PatientId { get; set; } 
        public Patient Patient { get; set; } 
        public string Notes { get; set; }
    }
}
