using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("=== HAOTIČNI KUPIDON - Korisnik ===");
Console.WriteLine();

string username = ReadNonEmptyString("Unesite username: ");
string city = ReadCity("Unesite grad: ");
int age = ReadPositiveInt("Unesite godine: ");
string phone = ReadPhoneNumber("Unesite broj telefona: ");


var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7001/cupidonHub")
    .Build();

connection.On<string, string, int, string, string>("ReceiveLetter",
    async (senderUsername, senderCity, senderAge, senderPhone, message) =>
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║              STIGLO LJUBAVNO PISMO!              ║");
        Console.WriteLine("╠══════════════════════════════════════════════════╣");
        Console.WriteLine($"║ Od:       {senderUsername,-39}║");
        Console.WriteLine($"║ Grad:     {senderCity,-39}║");
        Console.WriteLine($"║ Godine:   {senderAge,-39}║");

        if (senderPhone != "(skriveno)")
            Console.WriteLine($"║ Telefon:  {senderPhone,-39}║");
        else
            Console.WriteLine($"║ Telefon:  {"[nije dostupan]",-39}║");

        Console.WriteLine($"║ Poruka:   {message,-39}║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Pritisnite ENTER da potvrdite prijem pisma...");

        await Task.Run(() => Console.ReadLine());

        await connection.InvokeAsync("AcknowledgeLetter", username);
        Console.WriteLine("Prijem potvrđen. Čekamo sledeće pismo...");
        Console.WriteLine($"(Komanda za blokiranje: /block <username>)");
    });

try
{
    await connection.StartAsync();
    Console.WriteLine("Povezan na server.");
}
catch (Exception ex)
{
    Console.WriteLine($"Greška pri povezivanju: {ex.Message}");
    Console.WriteLine("Proverite da li server radi na https://localhost:7001");
    return;
}

string result;
try
{
    result = await connection.InvokeAsync<string>("InitSinglePerson", username, city, age, phone);
}
catch (Exception ex)
{
    Console.WriteLine($"Greška pri registraciji: {ex.Message}");
    return;
}

if (!result.StartsWith("OK"))
{
    Console.WriteLine($"Registracija neuspešna: {result}");
    return;
}

Console.WriteLine($"Uspešno registrovan/a kao '{username}'.");
Console.WriteLine($"  Grad: {city}, Godine: {age}, Telefon: {phone}");
Console.WriteLine();
Console.WriteLine("Čekamo pisma od Kupidona...");
Console.WriteLine("Komanda: /block <username>  ->  blokira korisnika");
Console.WriteLine();


while (true)
{
    string? input = Console.ReadLine();
    if (input == null) continue;

    input = input.Trim();

    if (input.StartsWith("/block ", StringComparison.OrdinalIgnoreCase))
    {
        string targetUsername = input[7..].Trim();

        if (string.IsNullOrWhiteSpace(targetUsername))
        {
            Console.WriteLine("Unesite username koji želite da blokirate. Primer: /block marko123");
            continue;
        }

        if (targetUsername == username)
        {
            Console.WriteLine("Ne možete blokirati sami sebe.");
            continue;
        }

        try
        {
            string blockResult = await connection.InvokeAsync<string>("BlockUser", username, targetUsername);

            if (blockResult.StartsWith("GRESKA"))
                Console.WriteLine($"{blockResult}");
            else
                Console.WriteLine($"Korisnik '{targetUsername}' blokiran. Nećete više primati pisma od njega/nje.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri blokiranju: {ex.Message}");
        }
    }
    else if (!string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Nepoznata komanda. Dostupne komande:");
        Console.WriteLine("  /block <username>  ->  blokira korisnika");
    }
}


static string ReadNonEmptyString(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("Polje ne sme biti prazno. Pokušajte ponovo.");
            continue;
        }

        return value.Trim();
    }
}


static int ReadPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Polje ne sme biti prazno. Unesite broj.");
            continue;
        }

        if (!int.TryParse(input, out int value))
        {
            Console.WriteLine("Unesite ispravan broj (samo cifre, bez slova ili karaktera).");
            continue;
        }

        if (value <= 0)
        {
            Console.WriteLine("Broj mora biti pozitivan (veći od nule).");
            continue;
        }

        return value;
    }
}

static string ReadCity(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("Polje ne sme biti prazno.");
            continue;
        }

        if (value.Any(char.IsDigit))
        {
            Console.WriteLine("Grad ne sme sadrzati brojeve.");
            continue;
        }

        return value.Trim();
    }
}

static string ReadPhoneNumber(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            Console.WriteLine("Broj telefona ne sme biti prazan.");
            continue;
        }

        string digitsOnly = value.StartsWith("+") ? value[1..] : value;

        if (!digitsOnly.All(char.IsDigit))
        {
            Console.WriteLine("Broj telefona sme sadrzati samo cifre (i opciono + na pocetku).");
            continue;
        }

        if (digitsOnly.Length < 6 || digitsOnly.Length > 15)
        {
            Console.WriteLine("Broj telefona mora imati izmedju 6 i 15 cifara.");
            continue;
        }

        return value.Trim();
    }
}
