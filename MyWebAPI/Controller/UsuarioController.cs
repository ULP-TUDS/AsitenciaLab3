using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyWebAPI.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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



       [HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromForm] LoginView loginView)
		{ 
			try
			{
				string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
					password: loginView.Clave,
					salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
					prf: KeyDerivationPrf.HMACSHA1,
					iterationCount: 1000,
					numBytesRequested: 256 / 8));
					Console.WriteLine( loginView.Usuario);

			
                  var p = await _context.Usuario
            .Include(u => u.Rol) // Incluye la propiedad de navegación Rol
            .FirstOrDefaultAsync(u => u.Email == loginView.Usuario && u.Rol.Id == loginView.Rol);
				if (p == null || p.Password != hashed)
				{
					return BadRequest("Nombre de usuario o clave incorrecta");
				}
				else
				{
					var key = new SymmetricSecurityKey(
						System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
					var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
					var claims = new List<Claim>
					{
						new Claim(ClaimTypes.Name, p.Email),
						new Claim("FullName", p.Nombre + " " + p.Apellido),
						new Claim(ClaimTypes.Role, p.Rol.Nombre),
						new Claim(ClaimTypes.NameIdentifier, p.UsuarioId.ToString())
					
						

						
					};

					var token = new JwtSecurityToken(
						issuer: config["TokenAuthentication:Issuer"],
						audience: config["TokenAuthentication:Audience"],
						claims: claims,
						expires: DateTime.Now.AddMinutes(60),
						signingCredentials: credenciales
					);
                    Console.WriteLine(p.Rol.Nombre);
					return Ok(new JwtSecurityTokenHandler().WriteToken(token));
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}




        		[HttpPut("put")]
		public async Task<IActionResult> Put( [FromBody] Usuario entidad)
		{   var user = HttpContext.User;

    var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

  
 if (int.TryParse(userIdClaim, out int id))
   {
    try
			{
				if (ModelState.IsValid)
				{
					entidad.UsuarioId = id;
					Usuario original = await _context.Usuario.FindAsync(id);
					if (String.IsNullOrEmpty(entidad.Password))
					{
						entidad.Password = original.Password;
					}
					else
					{
						entidad.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
							password: entidad.Password,
							salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
							prf: KeyDerivationPrf.HMACSHA1,
							iterationCount: 1000,
							numBytesRequested: 256 / 8));
					}
					_context.Entry(original).State = EntityState.Detached; 
					_context.Usuario.Update(entidad);

					await _context.SaveChangesAsync();
						return Ok(entidad);
					}
				return BadRequest();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
   }
    else
    {
        return BadRequest("Invalid user ID format in token");
    }
			
		}



    }

}
