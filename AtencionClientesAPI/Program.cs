using System.Text.Json.Serialization;
using AtencionClientesAPI.Controllers;
using AtencionClientesAPI.Data;
using AtencionClientesAPI.Hubs;
using AtencionClientesAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    });

builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<TurnoService>();
builder.Services.AddScoped<NotificacionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<TurnosHub>("/hubs/turnos");

app.Run();