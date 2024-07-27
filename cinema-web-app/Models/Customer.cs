using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class Customer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual ICollection<Reservation> Reservations { get; set; }
}