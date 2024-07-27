using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class ScreeningRoom
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; }
    public int TotalNoOfSeats { get; set; }
    public bool Is3D { get; set; }

    // Navigation properties
    [ForeignKey("CinemaId")] public Cinema Cinema { get; set; }

    public ICollection<Screening> Screenings { get; set; }
}