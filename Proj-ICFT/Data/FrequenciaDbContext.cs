using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;

namespace Proj_ICFT.Data
{
    public class FrequenciaDbContext : DbContext
    {
        public DbSet<Frequencia> Frequencia { get; set; }


        public FrequenciaDbContext(DbContextOptions<FrequenciaDbContext> options) : base(options) { }

    }
}
