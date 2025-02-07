using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.DTOs
{
    public class EmailSettings
    {
        public required string GmailUser { get; set; }
        
        public required string GmailAppPassword { get; set; }
    }
}
