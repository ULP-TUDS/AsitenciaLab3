using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class Turnos
    {
         [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Turno { get; set; }
    }
}