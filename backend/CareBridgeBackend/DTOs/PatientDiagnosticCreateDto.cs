using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.DTOs
{
    public class PatientDiagnosticCreateDto
    {
        [Required]
        public int DiagnosticTemplateId { get; set; }

        [Required]
        public int PatientId { get; set; }

        // The doctor creating the diagnostic is taken from the token
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateDiagnosed { get; set; }

        public string Notes { get; set; }
    }
}
