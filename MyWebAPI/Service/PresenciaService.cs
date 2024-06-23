using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

namespace MyWebAPI.Services
{
    public class PresenciaService
    {
        private readonly DataContext _context;

        public PresenciaService(DataContext context)
        {
            _context = context;
        }

        public async Task<Asistencia> GetAsistenciaPorTurnoAsync(string turno, bool isSemana, DateTime fecha)
        {
            Console.WriteLine($"Turno: {turno}, isSemana: {isSemana}, Fecha: {fecha}");
            
            var usuarios = await _context.Usuario
                .Include(u => u.Turnos)
                .Where(u => u.Turnos != null && u.Turnos.Turno == turno && u.Semana == isSemana)
                .ToListAsync();
            
            Console.WriteLine($"Usuarios: {string.Join(", ", usuarios.Select(u => u.UsuarioId))}");
            
            var idsUsuarios = usuarios.Select(u => u.UsuarioId).ToList();

            var idsUsuariosPresentes = await _context.Presencia
                .Where(p => idsUsuarios.Contains(p.UsuarioId) && p.Fecha.Date == fecha.Date)
                .Select(p => p.UsuarioId)
                .ToListAsync();

            Console.WriteLine($"Usuarios Presentes IDs: {string.Join(", ", idsUsuariosPresentes)}");

            var usuariosPresentes = usuarios
                .Where(u => idsUsuariosPresentes.Contains((int)u.UsuarioId))
                .Select(u => new UsuarioDetalles
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Turno = u.Turnos?.Turno,
                    TrabajaEnSemana = u.Semana,
                      Documento = u.Documento
                }).ToList();

            var usuariosAusentes = usuarios
                .Where(u => !idsUsuariosPresentes.Contains((int)u.UsuarioId))
                .Select(u => new UsuarioDetalles
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Turno = u.Turnos?.Turno,
                    TrabajaEnSemana = u.Semana,
                    Documento = u.Documento
                }).ToList();

            Console.WriteLine($"Usuarios Ausentes: {string.Join(", ", usuariosAusentes.Select(u => u.UsuarioId))}");

            return new Asistencia
            {
                usuarioPresentes = usuariosPresentes,
                usuarioAusentes = usuariosAusentes
            };
        }
    }
}
