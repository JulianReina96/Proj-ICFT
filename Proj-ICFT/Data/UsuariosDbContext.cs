using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;

namespace Proj_ICFT.Data
{
    public class UsuariosDbContext : DbContext
    {

        public DbSet<Usuarios> Usuarios { get; set; }

        public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }
    }
}
