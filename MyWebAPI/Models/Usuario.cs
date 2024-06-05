using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebAPI.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }
        
        [MaxLength(255)]
        public string Nombre { get; set; }

        [MaxLength(255)]
        public string Apellido { get; set; }

        [MaxLength(255)]
        public string Documento { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Telefono { get; set; }

        [MaxLength(255)]
        public string? Foto { get; set; }

          [ForeignKey(nameof(Puesto))]
        public int PuestoId { get; set; }
        public Puestos? Puesto { get; set; }

        
         [ForeignKey(nameof(Rol))]
         public int RolId { get; set; }
        public Rol? Rol { get; set; }

       [ForeignKey(nameof(Turnos))]
        public int TurnosId { get; set; }
        public Turnos? Turnos { get; set; }
       
       public bool Semana { get; set; }
       

       
    }
}
