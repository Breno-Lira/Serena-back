using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DominioUser; // namespace das entidades

namespace InfrastructureUser
{
    // DbContext é a classe principal do Entity Framework Core.
    // Ele representa a conexão com o banco e é responsável por mapear suas entidades.
    public class AppDbContext: DbContext
    {
        // O construtor recebe opções (ex.: string de conexão, provedor, lazy loading, etc).
        // Essas opções são configuradas no Program.cs ou Startup.cs.
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
        // DbSet representa uma coleção de todas as entidades no contexto, ou que podem ser consultadas do banco.
        public DbSet<User> Users { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Apoios> Apoios { get; set; }
        // aqui fazemos a configuração do banco, como  sua estrutura.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
