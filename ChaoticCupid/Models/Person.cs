namespace ChaoticCupid.Models
{
    public class Person
    {
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        public string ConnectionId { get; set; } = string.Empty;

        public bool WaitingForAcknowledgement { get; set; } = false;

        public List<string> BlockedUsers { get; set; } = new();
    }
}
