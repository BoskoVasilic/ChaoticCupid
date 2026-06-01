using ChaoticCupid.Hubs;
using ChaoticCupid.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PersonRegistry>();

builder.Services.AddSignalR();


var app = builder.Build();

app.MapHub<CupidonHub>("/cupidonHub");

Console.WriteLine("=== Haotični Kupidon - Server ===");
Console.WriteLine("Hub:   https://localhost:7001/cupidonHub");

app.Run();
