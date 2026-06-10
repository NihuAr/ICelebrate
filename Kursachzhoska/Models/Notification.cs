using System;

namespace Kursachzhoska.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool RequiresConfirmation { get; set; }
        public bool? IsConfirmed { get; set; } // null = не отвечено, true = подтверждено, false = отклонено
        public int? EventId { get; set; }
    }
}

