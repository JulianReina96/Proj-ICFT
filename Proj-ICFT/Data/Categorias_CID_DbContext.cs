using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{
    public class Categorias_CID_Dbcontext : DbContext
    {


        public DbSet<Categorias_CID> CategoriaCID { get; set; }


        public Categorias_CID_Dbcontext(DbContextOptions<Categorias_CID_Dbcontext> options) : base(options) { }

    }
}
