namespace CareBridgeBackend.Models
{
    public class Assistant : User
    {
        public ICollection<Doctor> Doctors { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
