using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{
    public class MedicamentosDbContext : DbContext
    {


        public DbSet<Medicamentos> Medicamentos { get; set; }


        public MedicamentosDbContext(DbContextOptions<MedicamentosDbContext> options) : base(options) { }

    }
}
