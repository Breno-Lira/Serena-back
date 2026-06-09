using DominioDenuncia;
using InfrastructureDenuncia;
using InfrastructureGeneric;
using Microsoft.EntityFrameworkCore;
using ServiceDenuncia;
using ServiceDenuncia.Profiles;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseUrls("https://localhost:7203","http://localhost:5226");

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não encontrada ou vazia. Verifique appsettings.json e o Startup Project.");
}
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.LogTo(Console.WriteLine, LogLevel.Information);
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString) // Use o provedor de banco de dados apropriado
);
builder.Services.AddMemoryCache();
builder.Services.AddScoped(typeof(IGenericRepository<Denuncia>), typeof(GenericRepositoryEntity<Denuncia,AppDbContext>));
builder.Services.AddScoped<IDenunciaService, DenunciaService>();
builder.Services.AddAutoMapper(typeof(DenunciaProfile));
builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UserApi:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(5); // timeout por request
});

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
