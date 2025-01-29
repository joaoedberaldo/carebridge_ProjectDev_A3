using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class DoctorAssistant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public User Doctor { get; set; }

        [Required]
        public int AssistantId { get; set; }
        [ForeignKey("AssistantId")]
        public User Assistant { get; set; }

        // Optional: Add a timestamp for when the assignment was made
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
