using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class Presencia
    {
        [Key]
        public int PresenciaId { get; set; }

         [ForeignKey(nameof(Usuario))]
         public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime Fecha { get; set; }
         
         [ForeignKey(nameof(Zona))]
          public int? ZonaId { get; set; }
        public Zona? Zona { get; set; }
        [NotMapped]
        public bool? isUpdate{ get; set; } 
    }
}