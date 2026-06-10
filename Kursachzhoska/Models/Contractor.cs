namespace Kursachzhoska.Models
{
    public class Contractor
    {
        public int ContractorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        public string FullName => $"{Name} {Surname}";
        public string DisplayText => $"{FullName} - {Category}";
    }
}

