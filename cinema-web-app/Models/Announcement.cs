using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class Announcement
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }

    // Navigation property
    [ForeignKey("CinemaId")] public Cinema Cinema { get; set; }
}