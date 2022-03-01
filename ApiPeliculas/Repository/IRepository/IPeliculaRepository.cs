using ApiPeliculas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Repository.IRepository
{
    public interface IPeliculaRepository
    {
        Task<ICollection<Pelicula>> GetPeliculas();
        Task<Pelicula> GetPelicula(int peliculaId);
        Task<ICollection<Pelicula>> GetPeliculasEnCategorias(int catId);
        Task<bool> CrearPelicula(Pelicula pelicula);
        Task<bool> ActualizarPelicula(Pelicula pelicula);
        Task<bool> BorrarPelicula(Pelicula pelicula);
        Task<IEnumerable<Pelicula>> BuscarPelicula(string nombre);
        Task<bool> ExistePelicula(string nombre);
        Task<bool> ExistePelicula(int id);
        Task<bool> Guardar();
    }
}
