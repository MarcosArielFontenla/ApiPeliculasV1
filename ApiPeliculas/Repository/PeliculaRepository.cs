using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Repository
{
    public class PeliculaRepository : IPeliculaRepository
    {
        private readonly ApplicationDbContext _db;

        public PeliculaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ICollection<Pelicula>> GetPeliculas()
        {
            return await _db.Peliculas.OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<Pelicula> GetPelicula(int peliculaId)
        {
            return await _db.Peliculas.FirstOrDefaultAsync(c => c.Id == peliculaId);
        }

        public async Task<ICollection<Pelicula>> GetPeliculasEnCategorias(int catId)
        {
            return await _db.Peliculas.Include(ca => ca.categoria)
                                      .Where(ca => ca.categoriaId == catId)
                                      .ToListAsync();
        }

        public async Task<bool> CrearPelicula(Pelicula pelicula)
        {
            _db.Peliculas.Add(pelicula);
            return await Guardar();
        }

        public async Task<bool> ActualizarPelicula(Pelicula pelicula)
        {
            _db.Peliculas.Update(pelicula);
            return await Guardar();
        }

        public async Task<bool> BorrarPelicula(Pelicula pelicula)
        {
            _db.Peliculas.Remove(pelicula);
            return await Guardar();
        }

        public async Task<IEnumerable<Pelicula>> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _db.Peliculas;

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
            }
            return await query.ToListAsync();
        }

        public async Task<bool> ExistePelicula(string nombre)
        {
            bool valor = await _db.Categorias.AnyAsync(c => c.Nombre.ToLower().Trim() == nombre);
            return valor;
        }

        public async Task<bool> ExistePelicula(int id)
        {
            return await _db.Peliculas.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> Guardar()
        {
            return await _db.SaveChangesAsync() >= 0 ? true : false;
        }
    }
}
