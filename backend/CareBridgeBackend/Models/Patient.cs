namespace CareBridgeBackend.Models
{
    public class Patient
    {
        public DateTime DateOfBirth { get; set; } 
        public string MedicalHistory { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
