using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;

namespace Proj_ICFT.Data
{
    public class InstrucoesAdicionaisDbContext : DbContext
    {
        public DbSet<InstrucoesAdicionais> InstrucoesAdicionais { get; set; }


        public InstrucoesAdicionaisDbContext(DbContextOptions<InstrucoesAdicionaisDbContext> options) : base(options) { }

    }
}
