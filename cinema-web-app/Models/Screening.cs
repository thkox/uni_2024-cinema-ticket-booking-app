using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class Screening
{
    public Guid Id { get; set; }
    public Guid ScreeningRoomId { get; set; }
    public Guid MovieId { get; set; }
    public DateTime StartTime { get; set; }
    public int RemainingNoOfSeats { get; set; }

    // Navigation properties
    [ForeignKey("ScreeningRoomId")] public ScreeningRoom ScreeningRoom { get; set; }

    [ForeignKey("MovieId")] public Movie Movie { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
}