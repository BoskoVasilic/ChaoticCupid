using ChaoticCupid.Models;

namespace ChaoticCupid.Services
{
    public class PersonRegistry
    {
        private readonly Dictionary<string, Person> _persons = new();

        private readonly object _lock = new();

        public bool TryRegister(Person person)
        {
            lock (_lock)
            {
                if (_persons.ContainsKey(person.Username))
                    return false;

                _persons[person.Username] = person;
                return true;
            }
        }

        public void RemoveByConnectionId(string connectionId)
        {
            lock (_lock)
            {
                var key = _persons.FirstOrDefault(p => p.Value.ConnectionId == connectionId).Key;
                if (key != null)
                    _persons.Remove(key);
            }
        }

        public List<Person> GetAll()
        {
            lock (_lock)
            {
                return _persons.Values.ToList();
            }
        }

        public Person? GetByUsername(string username)
        {
            lock (_lock)
            {
                _persons.TryGetValue(username, out var person);
                return person;
            }
        }

        public void SetWaiting(string username, bool waiting)
        {
            lock (_lock)
            {
                if (_persons.TryGetValue(username, out var person))
                    person.WaitingForAcknowledgement = waiting;
            }
        }

        public void BlockUser(string blockerUsername, string targetUsername)
        {
            lock (_lock)
            {
                if (_persons.TryGetValue(blockerUsername, out var blocker))
                {
                    if (!blocker.BlockedUsers.Contains(targetUsername))
                        blocker.BlockedUsers.Add(targetUsername);
                }
            }
        }
    }
}
