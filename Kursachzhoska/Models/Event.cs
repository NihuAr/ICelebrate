using System;
using System.Collections.Generic;

namespace Kursachzhoska.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty; // Для обратной совместимости (цвет)
        public string? EventImagePath { get; set; } // Путь к загруженному изображению
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string OrganizerCompanyName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public List<Contractor> Contractors { get; set; } = new List<Contractor>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
        public List<string> Documents { get; set; } = new List<string>(); // Пути к документам
        public bool IsFavorite { get; set; }
        public bool VisibleOnlyForCreator { get; set; }
    }
    
    public class EventParticipant
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }
}

