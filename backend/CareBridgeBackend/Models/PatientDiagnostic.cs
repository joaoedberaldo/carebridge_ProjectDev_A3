using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class PatientDiagnostic
    {
        public int Id { get; set; }

        [Required]
        public int DiagnosticTemplateId { get; set; } 
        public DiagnosticTemplate DiagnosticTemplate { get; set; }

        [Required]
        public int PatientId { get; set; } 
        public Patient Patient { get; set; }

        [Required]
        public int DoctorId { get; set; } 
        public Doctor Doctor { get; set; }


        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateDiagnosed { get; set; }
        public string Notes { get; set; }

        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
    }
}
