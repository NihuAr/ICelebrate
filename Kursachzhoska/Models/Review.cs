using System;

namespace Kursachzhoska.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; } // Оценка от 1 до 5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

