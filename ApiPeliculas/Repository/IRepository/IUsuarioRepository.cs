using ApiPeliculas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Repository.IRepository
{
    public interface IUsuarioRepository
    {
        Task<ICollection<Usuario>> GetUsuarios();
        Task<Usuario> GetUsuario(int usuarioId);
        Task<bool> ExisteUsuario(string usuario);
        Task<Usuario> Registro(Usuario usuario, string password);
        Task<Usuario> Login(string usuario, string password);
        Task<bool> Guardar();
    }
}
