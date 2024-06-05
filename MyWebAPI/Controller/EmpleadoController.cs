using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

namespace MyWebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadoController : ControllerBase
    {
         private readonly DataContext _context;
        private readonly IConfiguration config;

        public EmpleadoController(DataContext context,IConfiguration config)
        {
            _context = context;
            this.config = config;

        }
       [HttpPost("CargarAsistencia")]
[AllowAnonymous]
public async Task<IActionResult> CargarAsistencia([FromBody] Presencia presencia)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        if (presencia.PresenciaId == 0)
        {
            // Agregar nueva presencia
            _context.Add(presencia);
            await _context.SaveChangesAsync();
            return Ok(presencia);
        }
        else
        {
            // Buscar y eliminar presencia existente
            var existingPresencia = await _context.Presencia.FindAsync(presencia.PresenciaId);
            if (existingPresencia == null)
            {
                return NotFound("Presencia not found.");
            }

            _context.Remove(existingPresencia);
            await _context.SaveChangesAsync();
            return Ok(existingPresencia);
        }
    }
    catch (Exception ex)
    {
        // Log the exception (ex) here
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
    }
}

[HttpGet("ObtenerAsistenciasPorFecha")]
[AllowAnonymous]
public async Task<IActionResult> ObtenerAsistenciasPorFecha(DateTime fecha)
{
    try
    {
        var asistencias = await _context.Presencia
            .Include(p => p.Usuario)  // Incluye información del usuario
            .Where(p => p.Fecha.Date == fecha.Date)
            .ToListAsync();

        if (asistencias == null || !asistencias.Any())
        {
            return NotFound("No hay asistencias para la fecha proporcionada.");
        }

        return Ok(asistencias);
    }
    catch (Exception ex)
    {
        // Log the exception (ex) here
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
    }
}


    }
}