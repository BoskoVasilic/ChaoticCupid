using ChaoticCupid.Models;
using ChaoticCupid.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Cryptography;

namespace ChaoticCupid.Hubs
{
    public class CupidonHub : Hub
    {
        private readonly PersonRegistry _registry;

        public CupidonHub(PersonRegistry registry)
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

            Console.WriteLine($"[Hub] Subscriber: {username} ({city}, {age}g) -> 'love-letters'");
            return "OK";
        }

        public async Task AcknowledgeLetter(string username)
        {
            _registry.SetWaiting(username, false);
            Console.WriteLine($"[Hub] {username} potvrdio prijem.");
            await Task.CompletedTask;
        }

        public async Task<string> BlockUser(string blockerUsername, string targetUsername)
        {
            if (_registry.GetByUsername(targetUsername) == null)
                return $"GRESKA: Korisnik '{targetUsername}' ne postoji.";

            _registry.BlockUser(blockerUsername, targetUsername);
            Console.WriteLine($"[Hub] {blockerUsername} blokirao {targetUsername}.");
            return "OK";
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _registry.RemoveByConnectionId(Context.ConnectionId);
            Console.WriteLine($"[Hub] Konekcija prekinuta: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task PublishLetters()
        {
            Console.WriteLine("[Broker] Server je primio pisma...");

            var persons = _registry.GetAll();

            if (persons.Count < 2)
            {
                Console.WriteLine("[Broker] Nedovoljno prijavljenih osoba (min. 2).");
                return;
            }

            var messages = new[]
            {
            "Radujem se nasem susretu!",
            "Zelim da se upoznamo.",
            "Nisam zainteresovan/a za upoznavanje."
            };


            foreach (var recipient in persons)
            {
                if (recipient.WaitingForAcknowledgement)
                {
                    Console.WriteLine($"[Broker] {recipient.Username} jos ceka potvrdu - preskacemo.");
                    continue;
                }

                Person? bestSender = null;
                int bestScore = -1;

                foreach (var candidate in persons)
                {
                    if (candidate.Username == recipient.Username)
                        continue;

                    if (recipient.BlockedUsers.Contains(candidate.Username))
                        continue;

                    int score = 0;

                    if (string.Equals(candidate.City, recipient.City, StringComparison.OrdinalIgnoreCase))
                        score += 30;

                    if (Math.Abs(candidate.Age - recipient.Age) <= 2)
                        score += 20;

                    byte[] randomBytes = new byte[4];
                    RandomNumberGenerator.Fill(randomBytes);
                    score += (int)(BitConverter.ToUInt32(randomBytes, 0) % 101);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestSender = candidate;
                    }
                }

                if (bestSender == null)
                {
                    Console.WriteLine($"[Broker] Nema podesnog posiljaca za {recipient.Username}.");
                    continue;
                }

                byte[] bytes = new byte[4];
                RandomNumberGenerator.Fill(bytes);
                int msgIndex = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)messages.Length);
                string chosenMessage = messages[msgIndex];

                bool isNotInterested = chosenMessage == "Nisam zainteresovan/a za upoznavanje.";
                string phoneToSend = isNotInterested ? "(skriveno)" : bestSender.PhoneNumber;

                Console.WriteLine($"[Broker] {bestSender.Username} -> {recipient.Username} (score: {bestScore}) | \"{chosenMessage}\"");

                _registry.SetWaiting(recipient.Username, true);

                await Clients.Client(recipient.ConnectionId)
                .SendAsync("ReceiveLetter",
                    bestSender.Username,
                    bestSender.City,
                    bestSender.Age,
                    phoneToSend,
                    chosenMessage);
            }
        }
    }
}
