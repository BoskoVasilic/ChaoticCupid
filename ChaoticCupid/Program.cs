using ChaoticCupid.Hubs;
using ChaoticCupid.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PersonRegistry>();

builder.Services.AddSignalR();


var app = builder.Build();

app.MapHub<PersonHub>("/personHub");
app.MapHub<CupidonHub>("/cupidonHub");

Console.WriteLine("=== Haotični Kupidon - Server ===");
Console.WriteLine("PersonHub:   https://localhost:7001/personHub");
Console.WriteLine("CupidonHub: https://localhost:7001/cupidonHub");

app.Run();
