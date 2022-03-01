using ApiPeliculas.Models;
using ApiPeliculas.Models.DTOs;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Controllers
{
    [Authorize]
    [Route("api/Peliculas")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ApiPeliculas")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepository _pelRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PeliculasController(IPeliculaRepository pelRepository, IMapper mapper, IWebHostEnvironment hostEnvironment)
        {
            _pelRepository = pelRepository;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Obtiene todas las peliculas.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<PeliculaDTO>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPeliculas()
        {
            var listaPeliculas = await _pelRepository.GetPeliculas();
            var listaPeliculasDTO = new List<PeliculaDTO>();

            foreach (var lista in listaPeliculas)
            {
                listaPeliculasDTO.Add(_mapper.Map<PeliculaDTO>(lista));
            }
            return Ok(listaPeliculasDTO);
        }

        /// <summary>
        /// Obtiene pelicula por Id.
        /// </summary>
        /// <param name="peliculaId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(200, Type = typeof(PeliculaDTO))]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetPelicula(int peliculaId)
        {
            var itemPelicula = await _pelRepository.GetPelicula(peliculaId);

            if (itemPelicula == null)
                return NotFound();

            var itemPeliculaDTO = _mapper.Map<PeliculaDTO>(itemPelicula);
            return Ok(itemPeliculaDTO);
        }

        /// <summary>
        /// Obtiene pelicula por categoriaId
        /// </summary>
        /// <param name="categoriaId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("GetPeliculasEnCategoria/{categoriaId:int}")]
        public async Task<IActionResult> GetPeliculasenCategoria(int categoriaId)
        {
            var listaPelicula = await _pelRepository.GetPeliculasEnCategorias(categoriaId);

            if (listaPelicula == null)
                return NotFound();

            var itemPelicula = new List<PeliculaDTO>();

            foreach(var item in listaPelicula)
            {
                itemPelicula.Add(_mapper.Map<PeliculaDTO>(item));
            }
            return Ok(itemPelicula);
        }

        /// <summary>
        /// Busca pelicula por nombre.
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Buscar")]
        public async Task<IActionResult> Buscar(string nombre)
        {
            try
            {
                var result = await _pelRepository.BuscarPelicula(nombre);

                if (result.Any())
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error! recuperando datos de la app");
            }
        }

        /// <summary>
        /// Crea una pelicula.
        /// </summary>
        /// <param name="PeliculaDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaCreateDTO))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearPelicula([FromForm] PeliculaCreateDTO PeliculaDTO)
        {
            if (PeliculaDTO == null)
                return BadRequest(ModelState);

            if (await _pelRepository.ExistePelicula(PeliculaDTO.Nombre))
            {
                ModelState.AddModelError("", "La pelicula ya existe!");
                return StatusCode(404, ModelState);
            }

            // subida de archivos
            var archivo = PeliculaDTO.Foto;
            string rutaPrincipal = _hostEnvironment.WebRootPath;
            var archivos = HttpContext.Request.Form.Files;

            //if (archivo.Length > 0)
            //{
            //    // nueva imagen
            //    var nombreFoto = Guid.NewGuid().ToString();
            //    var subidas = Path.Combine(rutaPrincipal, @"fotos");
            //    var extension = Path.GetExtension(archivos[0].FileName);

            //    using (var fileStreams = new FileStream(Path.Combine(subidas, nombreFoto + extension), FileMode.Create))
            //    {
            //        archivos[0].CopyTo(fileStreams);
            //    }
            //    PeliculaDTO.RutaImagen = @"\fotos\" + nombreFoto + extension;
            //}
            var pelicula = _mapper.Map<Pelicula>(PeliculaDTO);

            if (!await _pelRepository.CrearPelicula(pelicula))
            {
                ModelState.AddModelError("", $"hubo un error, guardando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }

        /// <summary>
        /// Actualizar una pelicula existente.
        /// </summary>
        /// <param name="peliculaId"></param>
        /// <param name="peliculaDTO"></param>
        /// <returns></returns>
        [HttpPatch("{peliculaId:int}", Name = "ActualizarPelicula")]
        [ProducesResponseType(204)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPelicula(int peliculaId, [FromBody] PeliculaDTO peliculaDTO)
        {
            if (peliculaDTO == null || peliculaId != peliculaDTO.Id)
                return BadRequest(ModelState);

            var pelicula = _mapper.Map<Pelicula>(peliculaDTO);

            if (!await _pelRepository.ActualizarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Hubo un error, actualizando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        /// <summary>
        /// Borra una pelicula.
        /// </summary>
        /// <param name="peliculaId"></param>
        /// <returns></returns>
        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorrarPelicula(int peliculaId)
        {
            if (!await _pelRepository.ExistePelicula(peliculaId))
                return NotFound();

            var pelicula = await _pelRepository.GetPelicula(peliculaId);

            if (!await _pelRepository.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Hubo un error, borrando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
