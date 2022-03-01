using ApiPeliculas.Models;
using ApiPeliculas.Models.DTOs;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Controllers
{
    [Route("api/Categorias")]
    [ApiController]
    public class CategoriasController : Controller
    {
        private readonly ICategoriaRepository _ctRepository;
        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepository ctRepository, IMapper mapper)
        {
            _ctRepository = ctRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtener todas las categorias
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var listaCategorias = await _ctRepository.GetCategorias();
            var listaCategoriasDTO = new List<CategoriaDTO>();

            foreach(var lista in listaCategorias)
            {
                listaCategoriasDTO.Add(_mapper.Map<CategoriaDTO>(lista));
            }
            return Ok(listaCategoriasDTO);
        }

        /// <summary>
        /// Obtener categoria por Id
        /// </summary>
        /// <param name="categoriaId">Id de la categoria que recibo</param>
        /// <returns></returns>
        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        public async Task<IActionResult> GetCategoria(int categoriaId)
        {
            var itemCategoria = await _ctRepository.GetCategoria(categoriaId);

            if (itemCategoria == null)
                return NotFound();
            
            var itemCategoriaDTO = _mapper.Map<CategoriaDTO>(itemCategoria);
            return Ok(itemCategoriaDTO);
        }

        /// <summary>
        /// Crear una nueva categoria
        /// </summary>
        /// <param name="categoriaDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CategoriaDTO categoriaDTO)
        {
            if (categoriaDTO == null)
                return BadRequest(ModelState);

            if (await _ctRepository.ExisteCategoria(categoriaDTO.Nombre))
            {
                ModelState.AddModelError("", "La categoria ya existe!");
                return StatusCode(404, ModelState);
            }
            var categoria = _mapper.Map<Categoria>(categoriaDTO);

            if (!await _ctRepository.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"hubo un error, guardando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
        }

        /// <summary>
        /// Actualizar una categoria existente
        /// </summary>
        /// <param name="categoriaId"></param>
        /// <param name="categoriaDTO"></param>
        /// <returns></returns>
        [HttpPatch("{categoriaId:int}", Name = "ActualizarCategoria")]
        public async Task<IActionResult> ActualizarCategoria(int categoriaId, [FromBody] CategoriaDTO categoriaDTO)
        {
            if (categoriaDTO == null || categoriaId != categoriaDTO.Id)
                return BadRequest(ModelState);

            var categoria = _mapper.Map<Categoria>(categoriaDTO);

            if (!await _ctRepository.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Hubo un error, actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        /// <summary>
        /// Borrar una categoria
        /// </summary>
        /// <param name="categoriaId"></param>
        /// <returns></returns>
        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        public async Task<IActionResult> BorrarCategoria(int categoriaId)
        {
            if (!await _ctRepository.ExisteCategoria(categoriaId))
                return NotFound();

            var categoria = await _ctRepository.GetCategoria(categoriaId);

            if (!await _ctRepository.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Hubo un error, borrando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
