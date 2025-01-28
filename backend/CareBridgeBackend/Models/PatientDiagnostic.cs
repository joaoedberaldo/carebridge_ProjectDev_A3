namespace CareBridgeBackend.Models
{
    public class PatientDiagnostic
    {
        public int Id { get; set; } 
        public int DiagnosticTemplateId { get; set; } 
        public DiagnosticTemplate DiagnosticTemplate { get; set; }

        public int PatientId { get; set; } 
        public Patient Patient { get; set; } 

        public int DoctorId { get; set; } 
        public Doctor Doctor { get; set; } 

        public DateTime AssignedDate { get; set; }
        public string Notes { get; set; }

        public ICollection<Treatment> Treatments { get; set; }
    }
}
