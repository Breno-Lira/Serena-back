using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureDenuncia
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();

            // ATENÇÃO: Use a string de conexão COMPLETA e VÁLIDA da sua API.
            // Para Migrations e Update-Database funcionarem, o provedor deve ser o mesmo (UseSqlServer).
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Denuncia-API;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            builder.UseSqlServer(connectionString);

            // Esta instância criada é a que será usada pelas ferramentas do EF Core.
            return new AppDbContext(builder.Options);
        }
    }
}
