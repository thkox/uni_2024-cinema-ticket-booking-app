using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class ContentCinemaAdmin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CinemaId { get; set; }

    [ForeignKey("UserId")] public ApplicationUser User { get; set; }

    [ForeignKey("CinemaId")] public Cinema Cinema { get; set; }
}