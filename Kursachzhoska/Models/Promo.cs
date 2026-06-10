using System;

namespace Kursachzhoska.Models
{
    public class Promo
    {
        public int PromoId { get; set; }
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string OrganizerCompanyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BonusPrice { get; set; }
        public int UsageLimit { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }
        public int PurchasesCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
