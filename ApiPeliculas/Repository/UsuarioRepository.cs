using ApiPeliculas.Data;
using ApiPeliculas.Models;
using ApiPeliculas.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiPeliculas.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _db;

        public UsuarioRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ICollection<Usuario>> GetUsuarios()
        {
            return await _db.Usuarios.OrderBy(c => c.UsuarioA)
                                     .ToListAsync();
        }

        public async Task<Usuario> GetUsuario(int usuarioId)
        {
            return await _db.Usuarios.FindAsync(usuarioId);
        }

        public async Task<Usuario> Login(string usuario, string password)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.UsuarioA == usuario);

            if (user == null)
                return null;

            if (!VerificaPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }


        public async Task<Usuario> Registro(Usuario usuario, string password)
        {
            byte[] passwordHash, passwordSalt;
            CrearPasswordHash(password, out passwordHash, out passwordSalt);
            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;
            await _db.Usuarios.AddAsync(usuario);
            await Guardar();
            return usuario;
        }

        public async Task<bool> ExisteUsuario(string usuario)
        {
            if (await _db.Usuarios.AnyAsync(x => x.UsuarioA == usuario))
                return true;
            else
                return false;
        }

        public async Task<bool> Guardar()
        {
            return await _db.SaveChangesAsync() >= 0 ? true : false;
        }

        private bool VerificaPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var hashcomputado = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                for(int i = 0; i < hashcomputado.Length; i++)
                {
                    if (hashcomputado[i] != passwordHash[i])
                        return false;
                }
            }
            return true;
        }

        private void CrearPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                passwordSalt = hmac.Key;
            }
        }
    }
}
