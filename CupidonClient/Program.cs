using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("=== HAOTIČNI KUPIDON ===");

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7001/cupidonHub", options =>
    {
        options.HttpMessageHandlerFactory = _ => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    })
    .WithAutomaticReconnect()
    .Build();

try
{
    await connection.StartAsync();
    Console.WriteLine("Kupidon je povezan.");
    Console.WriteLine();
    Console.WriteLine("Kupidon ce automatski slati pisma svakog minuta.");
    Console.WriteLine("Komande:  send -> odmah pošalji  |  exit -> izlaz");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Greška pri povezivanju: {ex.Message}");
    return;
}

using var cts = new CancellationTokenSource();

var timerTask = Task.Run(async () =>
{
    Console.WriteLine("[Kupidon] Sledece automatsko slanje za 60 sekundi...");
    try { await Task.Delay(TimeSpan.FromMinutes(1), cts.Token); }
    catch (TaskCanceledException) { return; }

    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            Console.WriteLine("\n[Kupidon] Automatsko slanje pisama...");
            await connection.InvokeAsync("PublishLetters", cts.Token);
            Console.WriteLine("[Kupidon] Signal prosledjen. Sledece za 60 sekundi.\n");
        }
        catch (Exception ex) when (!cts.Token.IsCancellationRequested)
        {
            Console.WriteLine($"[Kupidon] Greska: {ex.Message}");
        }

        try { await Task.Delay(TimeSpan.FromMinutes(1), cts.Token); }
        catch (TaskCanceledException) { return; }
    }
}, cts.Token);

while (true)
{
    Console.Write("Kupidon> ");
    string? input = Console.ReadLine()?.Trim().ToLower();

    switch (input)
    {
        case "send":
            Console.WriteLine("Rucno slanje...");
            try
            {
                await connection.InvokeAsync("PublishLetters");
                Console.WriteLine("Signal prosleden.");
            }
            catch (Exception ex) { Console.WriteLine($"Greška: {ex.Message}"); }
            break;

        case "exit":
        case "quit":
            cts.Cancel();
            await connection.StopAsync();
            return;

        default:
            if (!string.IsNullOrWhiteSpace(input))
                Console.WriteLine("Nepoznata komanda. Koristite 'send' ili 'exit'.");
            break;
    }
}