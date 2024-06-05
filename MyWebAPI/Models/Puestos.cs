using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class Puestos
    {
            [Key]
        public int Id { get; set; }
        public int Puesto { get; set; }
    }
}