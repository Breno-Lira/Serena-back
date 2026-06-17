using InfrastructureUser;
using InfrastructureUser.Repositories;
using Microsoft.EntityFrameworkCore;
using ServiceUser;
using ServiceUser.Profiles;
using DominioUser;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Camada de apresentacao (API)
// ---------------------------------------------------------------------------
// ✅ Configuração do serviço de CORS (Adicionado para o Flutter Web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFlutterWeb", policy =>
    {
        policy.AllowAnyOrigin()   // Permite requisições do Chrome do Flutter
              .AllowAnyMethod()   // Permite POST, GET, etc.
              .AllowAnyHeader();  // Permite cabeçalhos como Content-Type
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------------------------
// Camada de servico
// ---------------------------------------------------------------------------
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddScoped<IUserService, UserService>();

// ---------------------------------------------------------------------------
// Camada de infraestrutura (banco + repositorio generico)
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' nao encontrada ou vazia. Verifique appsettings.json ou a variavel de ambiente ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<User>), typeof(GenericRepositoryEntity<User, AppDbContext>));

var app = builder.Build();

// ---------------------------------------------------------------------------
// Aplica as migrations no startup (cria o banco se necessario)
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations do banco de dados.");
    }
}

// ---------------------------------------------------------------------------
// Pipeline HTTP
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Ativação do middleware de CORS (Deve vir antes do UseAuthorization e MapControllers)
app.UseCors("PermitirFlutterWeb");

app.UseAuthorization();
app.MapControllers();

app.Run();
