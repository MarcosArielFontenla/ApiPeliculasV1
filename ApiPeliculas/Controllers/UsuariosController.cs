using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApiPeliculas.Repository.IRepository;
using ApiPeliculas.Models;
using ApiPeliculas.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace ApiPeliculas.Controllers
{
    [Authorize]
    [Route("api/Usuarios")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ApiPeliculasUsuarios")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository _usuRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor injectando Repository, Mapper y Configuration
        /// </summary>
        /// <param name="usuRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="configurationProvider"></param>
        public UsuariosController(IUsuarioRepository usuRepository, IMapper mapper, IConfiguration configurationProvider)
        {
            _usuRepository = usuRepository;
            _mapper = mapper;
            _config = configurationProvider;
        }

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<UsuarioDTO>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUsuarios()
        {
            var listaUsuarios = await _usuRepository.GetUsuarios();
            var listaUsuariosDTO = new List<UsuarioDTO>();

            foreach(var list in listaUsuarios)
            {
                listaUsuariosDTO.Add(_mapper.Map<UsuarioDTO>(list));
            }
            return Ok(listaUsuariosDTO);
        }

        /// <summary>
        /// Obtiene usuario por Id.
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        [HttpGet("{usuarioId:int}", Name = "GetUsuario")]
        [ProducesResponseType(200, Type = typeof(UsuarioDTO))]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetUsuario(int usuarioId)
        {
            var itemUsuario = await _usuRepository.GetUsuario(usuarioId);

            if (itemUsuario == null)
                return NotFound();

            var itemUsuarioDTO = _mapper.Map<UsuarioDTO>(itemUsuario);
            return Ok(itemUsuarioDTO);
        }

        /// <summary>
        /// Crear un usuario.
        /// </summary>
        /// <param name="usuarioAuthDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Registro")]
        public async Task<IActionResult> Registro(UsuarioAuthDTO usuarioAuthDTO)
        {
            usuarioAuthDTO.Usuario = usuarioAuthDTO.Usuario.ToLower();

            if (await _usuRepository.ExisteUsuario(usuarioAuthDTO.Usuario))
                return BadRequest("El usuario ya existe!");

            var usuarioACrear = new Usuario
            {
                UsuarioA = usuarioAuthDTO.Usuario
            };

            var usuarioCreado = await _usuRepository.Registro(usuarioACrear, usuarioAuthDTO.Password);
            return Ok(usuarioCreado);
        }

        /// <summary>
        /// Verifica el usuario si existe.
        /// </summary>
        /// <param name="usuarioAuthLoginDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UsuarioAuthLoginDTO usuarioAuthLoginDTO)
        {
            var usuarioDesdeRepo = await _usuRepository.Login(usuarioAuthLoginDTO.Usuario, usuarioAuthLoginDTO.Password);

            if (usuarioDesdeRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioDesdeRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, usuarioDesdeRepo.UsuarioA.ToString())
                };

            // generacion de token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credenciales,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}
