using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class ContentAppAdmin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
}