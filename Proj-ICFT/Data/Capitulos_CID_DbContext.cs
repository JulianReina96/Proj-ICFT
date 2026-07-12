using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{
    public class Capitulos_CID_Dbcontext : DbContext
    {


        public DbSet<Capitulos_CID> Capitulos_CID { get; set; }


        public Capitulos_CID_Dbcontext(DbContextOptions<Capitulos_CID_Dbcontext> options) : base(options) { }

    }
}
