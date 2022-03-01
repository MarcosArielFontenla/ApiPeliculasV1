using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Models.DTOs
{
    public class UsuarioDTO
    {
        public string UsuarioA { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}
