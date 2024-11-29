using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Models;
using System.Collections.Generic;

namespace Proj_ICFT.Data
{


    public class PacienteICTDbContext : DbContext
    {


        public DbSet<PacienteICT> Paciente { get; set; }
        public DbSet<Remedio_Paciente> Remedio_Pacientes { get; set; }


        public PacienteICTDbContext(DbContextOptions<PacienteICTDbContext> options) : base(options) { }

    }




    
}
