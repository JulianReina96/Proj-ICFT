using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{
    public class Blocos_CID_DbContext : DbContext
    {


        public DbSet<Blocos_CID> Blocos_CID { get; set; }


        public Blocos_CID_DbContext(DbContextOptions<Blocos_CID_DbContext> options) : base(options) { }

    }
}
