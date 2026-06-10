using System;

namespace Kursachzhoska.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TicketCount { get; set; }
        public bool NeedsCallback { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}

