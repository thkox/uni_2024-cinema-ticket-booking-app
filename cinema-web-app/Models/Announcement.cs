using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class Announcement
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public Guid UserId { get; set; } // New property for the author's UserId
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime PublicationDate { get; set; } // New property for the publication date

    // Navigation properties
    [ForeignKey("CinemaId")] public Cinema Cinema { get; set; }

    [ForeignKey("UserId")] public ApplicationUser User { get; set; } // Navigation property for the author
}