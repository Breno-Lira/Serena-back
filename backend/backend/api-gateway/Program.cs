using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseUrls("https://localhost:7213", "http://localhost:5175");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar a configuração do Ocelot a partir do arquivo 'ocelot.json'
// Isso deve vir antes de builder.Build()
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);


// Adicionar os serviços do Ocelot ao container de injeção de dependência
builder.Services.AddOcelot();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Usar o Ocelot Middleware
// Ele deve ser um dos primeiros middlewares a ser executado para rotear as requisições
await app.UseOcelot(); // O método é UseOcelot(), e ele deve ser await.

app.Run();
