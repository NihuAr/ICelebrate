using System;

namespace Kursachzhoska.Models
{
    public class UserPromo
    {
        public int UserPromoId { get; set; }
        public int UserId { get; set; }
        public int PromoId { get; set; }
        public string PromoTitle { get; set; } = string.Empty;
        public string UniqueCode { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public DateTime PurchasedAt { get; set; }
        public string? QRImageBase64 { get; set; }
    }
}
