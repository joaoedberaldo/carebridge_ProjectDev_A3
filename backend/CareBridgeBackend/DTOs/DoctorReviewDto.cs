namespace CareBridgeBackend.DTOs
{
    public class DoctorReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}
