using ChaoticCupid.Models;
using ChaoticCupid.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ChaoticCupid.Hubs
{
    public class PersonHub : Hub
    {
        private readonly PersonRegistry _registry;

        public PersonHub(PersonRegistry registry)
        {
            _registry = registry;
        }

        public async Task<string> InitSinglePerson(string username, string city, int age, string phoneNumber)
        {
            var person = new Person
            {
                Username = username,
                City = city,
                Age = age,
                PhoneNumber = phoneNumber,
                ConnectionId = Context.ConnectionId
            };

            if (!_registry.TryRegister(person))
                return $"GRESKA: Korisnik '{username}' vec postoji.";

            await Groups.AddToGroupAsync(Context.ConnectionId, "love-letters");
            Console.WriteLine($"[PersonHub] Subscriber registrovan: {username} ({city}, {age}g)");
            return "OK";
        }

        public async Task AcknowledgeLetter(string username)
        {
            _registry.SetWaiting(username, false);
            Console.WriteLine($"[PersonHub] {username} potvrdio prijem.");
            await Task.CompletedTask;
        }

        public async Task<string> BlockUser(string blockerUsername, string targetUsername)
        {
            if (_registry.GetByUsername(targetUsername) == null)
                return $"GRESKA: Korisnik '{targetUsername}' ne postoji.";

            _registry.BlockUser(blockerUsername, targetUsername);
            Console.WriteLine($"[PersonHub] {blockerUsername} blokirao {targetUsername}.");
            return "OK";
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _registry.RemoveByConnectionId(Context.ConnectionId);
            Console.WriteLine($"[PersonHub] Subscriber prekinuo vezu: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
