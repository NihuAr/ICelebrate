using System;

namespace Kursachzhoska.Models
{
    public class UserApplication
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public string Attendance { get; set; } = "unknown";
        public DateTime AppliedDate { get; set; }
    }
}
