using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareBridgeBackend.Models
{
    public class DoctorAssistant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        [Required]
        public int AssistantId { get; set; }

        [ForeignKey(nameof(AssistantId))]
        public User Assistant { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
