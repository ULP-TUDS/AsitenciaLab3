using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Models
{
    public class Zona
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Calle { get; set; }

        [MaxLength(255)]
        public string Desde { get; set; }

        [MaxLength(255)]
        public string Hasta { get; set; }
        public bool? seleccionada { get; set; }
    }
}