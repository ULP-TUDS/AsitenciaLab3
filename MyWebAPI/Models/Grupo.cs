using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class Grupo
    {
        public Usuario Empleado1 { get; set; }
        public Usuario Empleado2 { get; set; }

        public Zona zona { get; set; }
    }
}