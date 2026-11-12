using Microsoft.AspNetCore.SignalR.Client;
using System.Buffers.Text;
using System.Text.Json;

Console.WriteLine("CLIENTE");

var Url = "https://localhost:7222";

Console.Write("Modo: empleados / cliente: ");
var modo = Console.ReadLine()?.Trim().ToLowerInvariant();

if (modo != "empleados" && modo != "cliente")
{
    Console.WriteLine("Modo inválido.");
    return;
}

var clienteId = 0;

if (modo == "cliente")
{
    Console.Write("ID del cliente: ");

    if (!int.TryParse(Console.ReadLine(), out clienteId) ||
        clienteId <= 0)
    {
        Console.WriteLine("Error");
        return;
    }
}

await using var conexion = new HubConnectionBuilder()
    .WithUrl($"{Url}/hubs/turnos")
    .WithAutomaticReconnect()
    .Build();

void Mostrar(string evento, JsonElement datos)
{
    Console.WriteLine();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {evento}");

    Console.WriteLine(
        JsonSerializer.Serialize(
            datos,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }));
}

async Task SuscribirseAsync()
{
    if (modo == "empleados")
    {
        await conexion.InvokeAsync("UnirseEmpleados");
    }
    else
    {
        await conexion.InvokeAsync("UnirseCliente", clienteId);
    }
}

conexion.On<JsonElement>(
    "NuevoTurno",
    datos => Mostrar("NuevoTurno", datos));

conexion.On<JsonElement>(
    "TurnoActualizado",
    datos => Mostrar("TurnoActualizado", datos));

conexion.On<JsonElement>(
    "PosicionActualizada",
    datos => Mostrar("PosicionActualizada", datos));

conexion.Reconnecting += error =>
{
    Console.WriteLine();
    Console.WriteLine("Conexión interrumpida.");

    return Task.CompletedTask;
};

conexion.Reconnected += async connectionId =>
{
    try
    {
        await SuscribirseAsync();

        Console.WriteLine();
        Console.WriteLine("Reconectado");
        Console.WriteLine(
            "Consulta los endpoints REST para recuperar " +
            "los cambios ocurridos durante la desconexión.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"No se pudo recuperar la suscripción: {ex.Message}");
    }
};

conexion.Closed += error =>
{
    Console.WriteLine();
    Console.WriteLine(
        "Conexión cerrada. Vuelve a ejecutar el cliente para conectarte.");

    return Task.CompletedTask;
};

try
{
    await conexion.StartAsync();

    await SuscribirseAsync();

    Console.WriteLine();
    Console.WriteLine($"Conectado a {Url}/hubs/turnos");
    Console.WriteLine(
        modo == "empleados"
            ? "Escuchando notificaciones para empleados."
            : $"Escuchando notificaciones del cliente {clienteId}.");

    Console.WriteLine("Pulsa Enter para salir.");

    await Task.Run(() => Console.ReadLine());
}
catch (Exception ex)
{
    Console.WriteLine($"No se pudo conectar: {ex.Message}");
}
finally
{
    await conexion.StopAsync();
}