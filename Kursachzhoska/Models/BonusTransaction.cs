using System;

namespace Kursachzhoska.Models
{
    public class BonusTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? EventId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
