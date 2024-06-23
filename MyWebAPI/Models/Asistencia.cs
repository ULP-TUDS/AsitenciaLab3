using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class Asistencia
    {
         public List<UsuarioDetalles> usuarioPresentes { get; set; } = new List<UsuarioDetalles>();
        public List<UsuarioDetalles> usuarioAusentes { get; set; }  = new List<UsuarioDetalles>();
    }
}