using AutoMapper;
using InfrastructureUser;
using Microsoft.EntityFrameworkCore;
using ServiceUser;
using ServiceUser.Profiles;
using InfrastructureGeneric;
using DominioUser;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:7094","http://localhost:5223");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IDenunciaApiClient, DenunciaApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DenunciaApi:BaseUrl"] ?? throw new InvalidOperationException("UserApi:BaseUrl não configurado."));
    client.Timeout = TimeSpan.FromSeconds(5); // Defina um timeout apropriado
});

// 1. Obter a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não encontrada ou vazia. Verifique appsettings.json e o Startup Project.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString) // Use o provedor de banco de dados apropriado
);
builder.Services.AddScoped(typeof(IGenericRepository<User>), typeof(GenericRepositoryEntity<User,AppDbContext>));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Esta é a linha chave:
        // Se o banco não existir, ele cria o banco E todas as tabelas baseadas nas Models.
        // Se o banco já existir, ele não faz nada.
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao criar o banco de dados.");
    }
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
