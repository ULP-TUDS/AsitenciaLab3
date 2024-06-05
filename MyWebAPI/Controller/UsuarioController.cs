using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration config;

        public UsuarioController(DataContext context,IConfiguration config)
        {
            _context = context;
            this.config = config;

        }

        // GET: api/Usuario
        [HttpGet]
        
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios(int id)
        {
            
            return await _context.Usuario
                .Include(u => u.Turnos) // Incluye la propiedad de navegación Turno
                .ToListAsync();
        }
 [HttpGet("GetUsuariosTurno")]
public async Task<IActionResult> GetUsuariosTurno([FromBody] Filtrado filtrado)
{
    var usuarios = await _context.Usuario
        .Include(u => u.Turnos) // Incluye la propiedad de navegación Turno
        .Where(u => u.TurnosId == filtrado.turnoId && u.Semana == filtrado.isSemana)
        .ToListAsync();

    if (usuarios == null || !usuarios.Any())
    {
        return NotFound();
    }

    return Ok(usuarios);
}


[HttpPost("CargaPersonal")]
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
            password: usuario.Password,
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


    }

}
