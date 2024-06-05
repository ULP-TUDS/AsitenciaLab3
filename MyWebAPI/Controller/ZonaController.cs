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
public async Task<IActionResult> RecuperarSeleccionada([FromBody] Filtrado filtrado)
{
    try
    {
        var fechaActual = DateTime.Today;
        Console.WriteLine(fechaActual);

        // Recuperar zonas seleccionadas
        var zonasSeleccionadas = await _context.Zona
            .Where(z => z.seleccionada == true)
            .ToListAsync();

        var empleadosPresentes = await _context.Presencia
            .Include(p => p.Usuario.Turnos)
            .Where(p => p.Fecha.Date == fechaActual && p.Usuario.TurnosId == filtrado.turnoId && p.Usuario.Semana == filtrado.isSemana)
            .Select(p => p.Usuario)
            .ToListAsync();

        var random = new Random();
        var empleadosDesordenados = empleadosPresentes.OrderBy(x => random.Next()).ToList();
        var zonasDesordenadas = zonasSeleccionadas.OrderBy(x => random.Next()).ToList();
        
        var gruposUsuarios = new List<Grupo>();
        var updates = new List<Presencia>();

        for (int i = 0; i < empleadosDesordenados.Count; i += 2)
        {
            Grupo grupo = new Grupo
            {
                Empleado1 = empleadosDesordenados[i],
                zona = zonasDesordenadas[i / 2]  
            };

            if (i + 1 < empleadosDesordenados.Count)
            {
                grupo.Empleado2 = empleadosDesordenados[i + 1];
            }

            var asistenciaEmpleado1 = await _context.Presencia
                .FirstOrDefaultAsync(u => u.UsuarioId == grupo.Empleado1.UsuarioId);
            asistenciaEmpleado1.ZonaId = grupo.zona.Id;
            updates.Add(asistenciaEmpleado1);

            if (grupo.Empleado2 != null)
            {
                var asistenciaEmpleado2 = await _context.Presencia
                    .FirstOrDefaultAsync(u => u.UsuarioId == grupo.Empleado2.UsuarioId);
                asistenciaEmpleado2.ZonaId = grupo.zona.Id;
                updates.Add(asistenciaEmpleado2);
            }

            gruposUsuarios.Add(grupo);
        }

        _context.Presencia.UpdateRange(updates);
        await _context.SaveChangesAsync();

        return Ok(gruposUsuarios);
    }
    catch (Exception e)
    {
        // Registra la excepción (e) aquí usando un marco de registro
        return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error mientras se procesaba tu solicitud.");
    }
}


    }
}