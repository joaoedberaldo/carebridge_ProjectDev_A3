namespace CareBridgeBackend.DTOs
{
    public class OfficeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public List<int> DoctorIds { get; set; } = new List<int>();
    }
}
