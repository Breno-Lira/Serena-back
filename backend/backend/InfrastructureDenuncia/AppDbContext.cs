using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DominioDenuncia; // namespace das entidades

namespace InfrastructureDenuncia
{
    public class AppDbContext: DbContext
    {
        // O construtor recebe opções (ex.: string de conexão, provedor, lazy loading, etc).
        // Essas opções são configuradas no Program.cs ou Startup.cs.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        // DbSet representa uma coleção de todas as entidades no contexto, ou que podem ser consultadas do banco.
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        // aqui fazemos a configuração do banco, como  sua estrutura.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Denuncia>(entity =>
            {
                entity.ToTable("Denuncias");
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Status);

                entity.Property(d => d.TipoViolencia);

                entity.Property(d => d.UsuarioId).IsRequired(false);

                entity.Property(e => e.NomeDenunciante)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.Cpf)
                      .HasMaxLength(11)
                      .IsRequired(false);

                entity.HasOne(d => d.Endereco).WithOne(e => e.Denuncia).HasForeignKey<Endereco>(e => e.DenunciaId).OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.ToTable("Enderecos");
                entity.HasKey(d => d.Id);

            });
        }
    }
}
