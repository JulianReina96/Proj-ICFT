using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;

namespace Proj_ICFT.Data
{
    public class TipoDbContext : DbContext
    {
        public DbSet<Tipo> Tipo { get; set; }

        public TipoDbContext(DbContextOptions<TipoDbContext> options) : base(options) { }
    }
}
