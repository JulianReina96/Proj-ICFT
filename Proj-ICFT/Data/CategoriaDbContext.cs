using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{
    public class CategoriaDbContext : DbContext
    {


        public DbSet<Categoria> Categoria { get; set; }


        public CategoriaDbContext(DbContextOptions<CategoriaDbContext> options) : base(options) { }

    }
}
