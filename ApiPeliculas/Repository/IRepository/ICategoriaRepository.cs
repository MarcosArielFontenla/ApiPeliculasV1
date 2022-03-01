using ApiPeliculas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Repository.IRepository
{
    public interface ICategoriaRepository
    {
        Task<ICollection<Categoria>> GetCategorias();
        Task<Categoria> GetCategoria(int CategoriaId);
        Task<bool> CrearCategoria(Categoria categoria);
        Task<bool> ActualizarCategoria(Categoria categoria);
        Task<bool> BorrarCategoria(Categoria categoria);
        Task<bool> ExisteCategoria(string nombre);
        Task<bool> ExisteCategoria(int id);
        Task<bool> Guardar();
    }
}
