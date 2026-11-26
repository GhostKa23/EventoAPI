using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using EventosApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Banco em memória: os dados são recriados (seed) a cada execução.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase("EventosDb"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums aceitos como texto ("Publicado") ou número (2).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cria o banco em memória e aplica os dados iniciais (HasData).
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
