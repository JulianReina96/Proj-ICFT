using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;

namespace Proj_ICFT.Data
{
    public class InstrucoesAdicionaisPacienteDbContext : DbContext
    {

        public DbSet<InstrucoesAdicionais> Categoria { get; set; }


        public InstrucoesAdicionaisPacienteDbContext(DbContextOptions<InstrucoesAdicionaisPacienteDbContext> options) : base(options) { }
    }
}
