using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;
using WebAPIAutores.Migrations;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "getLibro")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            var libro = await context.Libros.Include(libro => libro.AutoresLibros)
                                            .ThenInclude(autorLibro => autorLibro.Autor)
                                            .Include(libro => libro.Comentarios)
                                            .FirstOrDefaultAsync(libro => libro.Id == id);

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(autores => autores.Orden).ToList();

            return mapper.Map<LibroDTO>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresIds = await context.Autores.Where(autor => libroCreacionDTO.AutoresIds.Contains(autor.Id)).Select(autor => autor.Id).ToListAsync();

            if ( libroCreacionDTO.AutoresIds.Count != autoresIds.Count )
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);

            if (libro.AutoresLibros != null)
            {
                for(int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

            var libroDTO = mapper.Map<LibroDTO>(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            return CreatedAtRoute("getLibro", new { Id = libro.Id}, libroDTO);
        }
    }
}
