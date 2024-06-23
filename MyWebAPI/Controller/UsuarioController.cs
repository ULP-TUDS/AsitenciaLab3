using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyWebAPI.Models;
using MyWebAPI.Services;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyWebAPI.Controllers
{  
    [Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
    [Authorize(Policy = "Administrador")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration config;
          private readonly PresenciaService _presenciaService;

        private readonly IWebHostEnvironment environment;
  


        public UsuarioController(DataContext context,IConfiguration config, IWebHostEnvironment env,PresenciaService presenciaService)
        {
            _context = context;
            this.config = config;
            environment=env;
             _presenciaService = presenciaService;

        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios(int id)
        {
            
            return await _context.Usuario
                .Include(u => u.Turnos) // Incluye la propiedad de navegación Turno
                .ToListAsync();
        }

  [HttpGet("GetUsuariosTurno")]
    
        public async Task<ActionResult<Asistencia>> GetUsuarios([FromQuery] string turno, [FromQuery] bool isSemana)
        {  
           DateTime start = DateTime.Now;
            var asistencia = await _presenciaService.GetAsistenciaPorTurnoAsync(turno, isSemana, start);
            return Ok(asistencia);
        }

 [HttpGet("GetUserAll")]
        public async Task<ActionResult<List<Usuario>>> GetUserAll([FromQuery] string turno, [FromQuery] bool isSemana)
        {
            var usuarios = await _context.Usuario
                                         .Include(u => u.Turnos) // Incluye la entidad relacionada Turnos
                                         .Include(u => u.Puesto)
                                         .Include(u => u.Rol) // Incluye la entidad relacionada Puesto
                                         .Where(u => u.Turnos.Turno == turno && u.Semana == isSemana)
                                         .ToListAsync();

            return Ok(usuarios);
        }
[HttpPost("CargarAsistencia")]

public async Task<IActionResult> CargarAsistencia(
    [FromForm] DateTime fecha,
    [FromForm] int usuarioId)
{
  

    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
     
        var existingPresencia = await _context.Presencia
            .FirstOrDefaultAsync(p => p.Fecha == fecha && p.UsuarioId == usuarioId);

        if (existingPresencia == null)
        {
          
            Presencia nuevaPresencia = new Presencia
            {
                Fecha = fecha,
                UsuarioId = usuarioId,
           
            };

            _context.Add(nuevaPresencia);
            await _context.SaveChangesAsync();
            return Ok(nuevaPresencia);
        }
        else
        {
         
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


 [HttpGet("usuario")]
        public async Task<IActionResult> Usuario()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Rol) // Incluye la propiedad de navegación Rol si es necesario
                .FirstOrDefaultAsync(u => u.UsuarioId == int.Parse(userId));

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }





[HttpGet("usuario/{id}")]


public async Task<IActionResult> Usuario(int id)
{
    var usuario = await _context.Usuario
        .Include(u => u.Rol) // Incluye la propiedad de navegación Rol si es necesario
        .FirstOrDefaultAsync(u => u.UsuarioId == id);

    if (usuario == null)
    {
        return NotFound();
    }

    return Ok(usuario);
}


//-----------------------------------------------------------------------------------------------------------------------------------------
[HttpPost("cargarupdate")]
public async Task<IActionResult> CargarUpdate([FromForm] IFormFile imagen, [FromForm] string usuario)
{
    try
    {
        // Deserializa el JSON al objeto Usuario
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var usuarioObject = JsonSerializer.Deserialize<Usuario>(usuario, options);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Obtiene el ID del usuario autenticado
        var user = HttpContext.User;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int propietarioId))
        {
            return BadRequest("Invalid user ID format in token");
        }

        // Busca el usuario existente en la base de datos
        var usuarioencontrado = await _context.Usuario.FindAsync(propietarioId);
        if (usuarioencontrado == null)
        {
            return NotFound("User not found");
        }

        // Actualiza las propiedades del usuario existente
        usuarioencontrado.Nombre = usuarioObject.Nombre;
        usuarioencontrado.Apellido = usuarioObject.Apellido;
        usuarioencontrado.Email = usuarioObject.Email;
         usuarioencontrado.Documento = usuarioObject.Documento;
         usuarioencontrado.Domicilio = usuarioObject.Domicilio;
          usuarioencontrado.Telefono = usuarioObject.Telefono;

        if (usuarioObject.Password!="")
        {
          usuarioencontrado.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: usuarioObject.Password,
            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8));
        }
      
       
        

        // Si se ha subido una imagen, actualiza la propiedad Foto
        if (imagen != null)
        {
            var uploadsRootFolder = Path.Combine(environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsRootFolder))
            {
                Directory.CreateDirectory(uploadsRootFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            var filePath = Path.Combine(uploadsRootFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(fileStream);
            }

            usuarioencontrado.Foto = Path.Combine("uploads", uniqueFileName);
        }

        // Guarda los cambios en la base de datos
        _context.Usuario.Update(usuarioencontrado);
        await _context.SaveChangesAsync();

        return Ok(usuarioencontrado);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
//------------------------------------------------------------------------------------------------------------------------
[HttpPost("sinimagen")]
public async Task<IActionResult> Sinimagen([FromBody] Usuario usuario)
{

    try
    {
        var user = HttpContext.User;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int propietarioId))
        {
          
            return BadRequest("Invalid user ID format in token");
        }

        var usuarioencontrado = await _context.Usuario.FindAsync(propietarioId);
        if (usuarioencontrado == null)
        {
            return NotFound("User not found");
        }

        usuarioencontrado.Nombre = usuario.Nombre;
        usuarioencontrado.Apellido = usuario.Apellido;
        usuarioencontrado.Email = usuario.Email;
        usuarioencontrado.Documento = usuario.Documento;
        usuarioencontrado.Domicilio = usuario.Domicilio;
        usuarioencontrado.Telefono = usuario.Telefono;

        // Solo actualizar la contraseña si se proporciona una nueva
        if (!string.IsNullOrEmpty(usuario.Password))
        {
            usuarioencontrado.Password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: usuario.Password,
                salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
        }

        _context.Usuario.Update(usuarioencontrado);
        await _context.SaveChangesAsync();

        return Ok(usuarioencontrado);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Internal server error: " + ex.Message);
    }
}

//----------------------------------------------------------------------------------------------------------------------------------------------
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

        var p = await _context.Usuario
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == loginView.Usuario && u.Rol.Nombre == loginView.Rol);

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

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
    catch (Exception ex)
    {
        // Manejo de excepciones según necesidad
        return StatusCode(StatusCodes.Status500InternalServerError, "Error en el servidor");
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
