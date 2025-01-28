namespace CareBridgeBackend.Models
{
    public class Assistant : User
    {
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
