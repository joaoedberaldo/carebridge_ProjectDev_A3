using System.ComponentModel.DataAnnotations;

namespace CareBridgeBackend.Models
{
    public class Office
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(20)]
        public string ZipCode { get; set; }

        public ICollection<User> Doctors { get; set; } = new List<User>();
    }
}
