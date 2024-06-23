using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

namespace MyWebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmpleadoController : ControllerBase
    {
         private readonly DataContext _context;
        private readonly IConfiguration config;

        public EmpleadoController(DataContext context,IConfiguration config)
        {
            _context = context;
            this.config = config;

        }
 
  
        [HttpGet("getpresentes")]
        [Authorize(Policy ="AdministradorOEmpleado")]
      
        public async Task<IActionResult> GetPresenciasByUsuarioId()
        {
            // Obtener el ID del usuario autenticado y convertirlo a entero
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            if (!int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("No se pudo obtener el ID del usuario.");
            }

            try
            {
                // Obtener la fecha de hoy y el primer día del mes actual
             
                var today = DateTime.Now;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // Consultar presencias del usuario dentro del mes actual
                var presencias = await _context.Presencia
                    .Where(p => p.UsuarioId == usuarioId && p.Fecha >= startOfMonth && p.Fecha <= today)
                    .ToListAsync();

                // Verificar si se encontraron presencias
                if (presencias == null || !presencias.Any())
                { 
                    return NotFound("No se encontraron presencias para el usuario en el mes actual.");
                }

           
                return Ok(presencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    
[HttpPost("updateEmpleado")]
 // [Authorize(Policy ="Administrador")]
 [AllowAnonymous]

public async Task<IActionResult> UpdateEmpleado( [FromBody] Usuario updatedUsuario)
{
    if (updatedUsuario == null || updatedUsuario.UsuarioId == 0)
    {
        return BadRequest("Invalid user data");
    }

    var existingUsuario = await _context.Usuario.FindAsync(updatedUsuario.UsuarioId);
    if (existingUsuario == null)
    {
        return NotFound("User not found");
    }

    // Update the fields
    existingUsuario.Nombre = updatedUsuario.Nombre;
    existingUsuario.Apellido = updatedUsuario.Apellido;
    existingUsuario.Documento = updatedUsuario.Documento;
    existingUsuario.Email = updatedUsuario.Email;
    existingUsuario.Telefono = updatedUsuario.Telefono;
    existingUsuario.PuestoId = updatedUsuario.PuestoId;
    existingUsuario.TurnosId = updatedUsuario.TurnosId;
    existingUsuario.Semana = updatedUsuario.Semana;
    existingUsuario.Domicilio = updatedUsuario.Domicilio;

   

    try
    {
        await _context.SaveChangesAsync();
        return Ok(existingUsuario);
    }
    catch (Exception ex)
    {
        // Handle the exception
        return StatusCode(500, "Internal server error");
    }
}
[HttpPost("CargaPersonal")]
//  [Authorize(Policy ="Administrador")]
[AllowAnonymous]
public async Task<IActionResult> CargaPersonal([FromBody] Usuario usuario)
{  
    if (!ModelState.IsValid)
    { 
        return BadRequest(ModelState);
    }

    try
    {
        var mailexiste = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Email == usuario.Email);
        if (mailexiste != null)
        {
            return BadRequest("El email ya está registrado.");
        }
        usuario.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: usuario.Documento,
            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8));

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(usuario);
    }
    catch (Exception ex)
    {
        // Log the exception (ex) here
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
    }
}


[HttpGet("ObtenerAsistenciasPorFecha")]
  
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