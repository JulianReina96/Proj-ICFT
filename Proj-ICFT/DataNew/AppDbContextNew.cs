using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.DataNew;

public partial class AppDbContextNew : DbContext
{
    public AppDbContextNew(DbContextOptions<AppDbContextNew> options)
        : base(options)
    {
    }

    public virtual DbSet<Blocos_CID> Blocos_CIDs { get; set; }

    public virtual DbSet<Capitulos_CID> Capitulos_CIDs { get; set; }

    public virtual DbSet<Categorias_CID> Categorias_CIDs { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Frequencium> Frequencia { get; set; }

    public virtual DbSet<InstrucoesAdicionai> InstrucoesAdicionais { get; set; }

    public virtual DbSet<InstrucoesMed> InstrucoesMeds { get; set; }

    public virtual DbSet<Medicamento> Medicamentos { get; set; }

    public virtual DbSet<PacienteICT> PacienteICTs { get; set; }

    public virtual DbSet<ReceitaCID> ReceitaCIDs { get; set; }

    public virtual DbSet<ReceitaMed> ReceitaMeds { get; set; }

    public virtual DbSet<Receitum> Receita { get; set; }

    public virtual DbSet<Tipo> Tipos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<EvolucaoClinica> EvolucaoClinicas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blocos_CID>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Blocks__3214EC07C8C74789");

            entity.HasOne(d => d.ChapterNoNavigation).WithMany(p => p.Blocos_CIDs)
                .HasPrincipalKey(p => p.ChapterNo)
                .HasForeignKey(d => d.ChapterNo)
                .HasConstraintName("FK_Blocos_CID_Capitulos_CID");
        });

        modelBuilder.Entity<Capitulos_CID>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Chapters__3214EC0741E1B9B3");
        });

        modelBuilder.Entity<Categorias_CID>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC0775CEAE9D");

            entity.HasOne(d => d.ChapterNoNavigation).WithMany(p => p.Categorias_CIDs)
                .HasPrincipalKey(p => p.ChapterNo)
                .HasForeignKey(d => d.ChapterNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categorias_CID_Capitulos_CID");
        });

        modelBuilder.Entity<InstrucoesMed>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_InstrucoesAdicionais_Paciente");

            entity.HasOne(d => d.Instrucao).WithMany(p => p.InstrucoesMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Instrucoes_InstrucoesAdicionais");

            entity.HasOne(d => d.Med_Receita).WithMany(p => p.InstrucoesMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Instrucoes_Med_Receita");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medicame__3214EC079B86793D");
        });

        modelBuilder.Entity<PacienteICT>(entity =>
        {
            entity.Property(e => e.Sexo).IsFixedLength();

            entity.HasOne(d => d.UsuarioCriacao).WithMany(p => p.PacienteICTs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PacienteICT_Usuarios");
        });

        modelBuilder.Entity<ReceitaCID>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Categoria_CID_Receita");

            entity.HasOne(d => d.CategoriaCID).WithMany(p => p.ReceitaCIDs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CID_Receita_Categorias_CID");

            entity.HasOne(d => d.Receita).WithMany(p => p.ReceitaCIDs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CID_Receita_Receita");
        });

        modelBuilder.Entity<ReceitaMed>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_Remedio_Paciente");

            entity.HasOne(d => d.Categoria).WithMany(p => p.ReceitaMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Receita_Categoria");

            entity.HasOne(d => d.Frequencia).WithMany(p => p.ReceitaMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Receita_Frequencia");

            entity.HasOne(d => d.Medicamento).WithMany(p => p.ReceitaMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Receita_Medicamentos");

            entity.HasOne(d => d.Receita).WithMany(p => p.ReceitaMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Receita_Receita");

            entity.HasOne(d => d.Tipo).WithMany(p => p.ReceitaMeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Med_Receita_Tipo");
        });

        modelBuilder.Entity<Receitum>(entity =>
        {
            entity.HasOne(d => d.Paciente).WithMany(p => p.Receita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receita_PacienteICT");

            entity.HasOne(d => d.UsuarioCriacao).WithMany(p => p.Receita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receita_Usuarios");
        });

        modelBuilder.Entity<Tipo>(entity =>
        {
            entity.HasOne(d => d.Categoria).WithMany(p => p.Tipos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tipo_Categoria");
        });

        modelBuilder.Entity<EvolucaoClinica>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Paciente).WithMany(p => p.EvolucaoClinicas)
                .HasForeignKey(e => e.PacienteID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EvolucaoClinica_PacienteICT");

            entity.HasOne(e => e.Receita).WithMany(r => r.EvolucaoClinicas)
                .HasForeignKey(e => e.ReceitaID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_EvolucaoClinica_Receita");

            entity.HasOne(e => e.CategoriaCID).WithMany(c => c.EvolucaoClinicas)
                .HasForeignKey(e => e.CategoriaCID_ID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_EvolucaoClinica_Categorias_CID");

            entity.HasOne(e => e.UsuarioCriacao).WithMany(u => u.EvolucaoClinicas)
                .HasForeignKey(e => e.UsuarioCriacaoID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_EvolucaoClinica_Usuario");

            entity.HasIndex(e => e.PacienteID).HasDatabaseName("IX_EvolucaoClinica_PacienteID");
            entity.HasIndex(e => new { e.PacienteID, e.DataConsulta })
                  .HasDatabaseName("IX_EvolucaoClinica_Paciente_Data");

            entity.Property(e => e.Status).HasConversion<int>();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
