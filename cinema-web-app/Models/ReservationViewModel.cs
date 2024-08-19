namespace cinema_web_app.Models;

public class ReservationViewModel
{
    public Guid MovieId { get; set; }
    public Guid CinemaId { get; set; }
    public Guid ScreeningId { get; set; }
    public DateTime ScreeningDate { get; set; }
    public int NoOfSeats { get; set; }

    public string MovieTitle { get; set; }
    public string CinemaName { get; set; }
    public string ScreeningDetails { get; set; }
}