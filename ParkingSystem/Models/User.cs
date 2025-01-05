using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class User
    {
        [Key]
    public int Id { get; set; }  

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;  

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();


    }
}
