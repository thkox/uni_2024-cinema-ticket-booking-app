using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class ApplicationAdmin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey("UserId")] public ApplicationUser User { get; set; }
}