namespace CareBridgeBackend.Models
{
    public class Doctor : User
    {
        public string Speciality { get; set; } 
        public ICollection<Assistant> Assistants { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
