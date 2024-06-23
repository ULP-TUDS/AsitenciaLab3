using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.Models;

namespace MyWebAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonaController : ControllerBase
    {
          private readonly DataContext _context;
        private readonly IConfiguration config;

        public ZonaController(DataContext context,IConfiguration config)
        {
            _context = context;
            this.config = config;

        }
 [HttpGet("zonas")]

        public async Task<IActionResult> GetZonas()
        {
            var zonas = await _context.Zona.ToListAsync();
            return Ok(zonas);
        } 

        
 [HttpPut("putzona")]
public async Task<IActionResult> PutZonas([FromBody] Zona zona)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        var zonas = await _context.Zona
            .FirstOrDefaultAsync(u => u.Id == zona.Id);
        if (zonas == null)
        {
            return BadRequest("La zona no existe");
        }

     
        zonas.seleccionada = zona.seleccionada;
       

        _context.Zona.Update(zonas);
        await _context.SaveChangesAsync();

        return Ok(zonas);
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
    }
}

        



        
        [HttpPost("CargarZona")]
        public async Task<IActionResult>  CargarZona([FromBody]Zona zona)
        {
             if (!ModelState.IsValid){

                
                return BadRequest(ModelState);

             }
             else{

               try{
                 
                _context.Zona.Add(zona);
                 await _context.SaveChangesAsync();
                  return Ok(zona);

               }
               catch{
return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");

               }

             }

         
        }

[HttpGet("RecuperarSeleccionada")]
public async Task<IActionResult> RecuperarSeleccionada([FromQuery] string turno, [FromQuery] bool isSemana)
{

    try
    {
        var fechaActual = DateTime.Today;


        // Recuperar zonas seleccionadas
        var zonasSeleccionadas = await _context.Zona
            .Where(z => z.seleccionada == true)
            .ToListAsync();

        var empleadosPresentes = await _context.Presencia
            .Include(p => p.Usuario.Turnos)
            .Where(p => p.Fecha.Date == fechaActual && p.Usuario.Turnos.Turno == turno && p.Usuario.Semana == isSemana)
            .Select(p => p.Usuario)
            .ToListAsync();

        var random = new Random();
        var empleadosDesordenados = empleadosPresentes.OrderBy(x => random.Next()).ToList();
        var zonasDesordenadas = zonasSeleccionadas.OrderBy(x => random.Next()).ToList();
        
        var gruposResponse = new List<GrupoResponse>();
        var updates = new List<Presencia>();

        for (int i = 0; i < empleadosDesordenados.Count; i += 2)
        {
            if (i / 2 >= zonasDesordenadas.Count)
            {
                break; // Asegúrate de no intentar acceder a una zona que no existe
            }

            var zona = zonasDesordenadas[i / 2];
            var empleado1 = empleadosDesordenados[i];
            var empleado2 = i + 1 < empleadosDesordenados.Count ? empleadosDesordenados[i + 1] : null;

            var grupoResponse = new GrupoResponse
            {
                NombreEmpleado1 = $"{empleado1.Nombre} {empleado1.Apellido}",
                NombreEmpleado2 = empleado2 != null ? $"{empleado2.Nombre} {empleado2.Apellido}" : null,
                calle = zona.Calle,
                Desde = zona.Desde,
                Hasta = zona.Hasta
            };

            var asistenciaEmpleado1 = await _context.Presencia
                .FirstOrDefaultAsync(u => u.UsuarioId == empleado1.UsuarioId && u.Fecha.Date == fechaActual);
            if (asistenciaEmpleado1 != null)
            {
                asistenciaEmpleado1.ZonaId = zona.Id;
                updates.Add(asistenciaEmpleado1);
            }

            if (empleado2 != null)
            {
                var asistenciaEmpleado2 = await _context.Presencia
                    .FirstOrDefaultAsync(u => u.UsuarioId == empleado2.UsuarioId && u.Fecha.Date == fechaActual);
                if (asistenciaEmpleado2 != null)
                {
                    asistenciaEmpleado2.ZonaId = zona.Id;
                    updates.Add(asistenciaEmpleado2);
                }
            }

            gruposResponse.Add(grupoResponse);
        }

        _context.Presencia.UpdateRange(updates);
        await _context.SaveChangesAsync();
     
        return Ok(gruposResponse);
    }
    catch (Exception e)
    {
        // Registra la excepción (e) aquí usando un marco de registro
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error mientras se procesaba tu solicitud.");
    }
}

    }
}