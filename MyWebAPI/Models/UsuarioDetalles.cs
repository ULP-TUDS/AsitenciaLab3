using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebAPI.Models
{
    public class UsuarioDetalles
    {
        public int? UsuarioId { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Turno { get; set; }
    public bool? TrabajaEnSemana { get; set; }
    public string? Documento { get; set; }
    }
}