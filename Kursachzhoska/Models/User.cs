using System;
using System.Collections.Generic;

namespace Kursachzhoska.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Preferences { get; set; } = new List<string>();
        public bool IsOrganizer { get; set; }
        public string? CompanyName { get; set; }
        public List<string> OrganizerCategories { get; set; } = new List<string>();
        public List<int> FavoriteEventIds { get; set; } = new List<int>();
        public string? ProfileImagePath { get; set; }
        public int BonusBalance { get; set; }
        public bool Is2FAEnabled { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}

