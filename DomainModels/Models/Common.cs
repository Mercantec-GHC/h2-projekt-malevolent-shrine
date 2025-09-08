using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModels.Models
{
    public class Common
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } // Убираем = DateTime.Now
        public DateTime UpdatedAt { get; set; } // Убираем = DateTime.Now
    }
}