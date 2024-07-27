using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_web_app.Models;

public class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int GenreId { get; set; }
    public int Duration { get; set; }
    public string Content { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; }
    public string ImageUrl { get; set; }

    // Navigation property
    [ForeignKey("GenreId")] public Genre Genre { get; set; }

    public ICollection<Screening> Screenings { get; set; }
}