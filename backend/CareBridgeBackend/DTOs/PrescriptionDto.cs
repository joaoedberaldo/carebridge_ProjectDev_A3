using CareBridgeBackend.Models;

namespace CareBridgeBackend.DTOs
{
    public class PrescriptionDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public PrescriptionStatus Status { get; set; }
        public int? AppointmentId { get; set; }
        public List<MedicationDto> Medications { get; set; } = new List<MedicationDto>();
    }
}
