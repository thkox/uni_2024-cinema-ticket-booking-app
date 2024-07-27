namespace cinema_web_app.Models;
using System;
using System.Collections.Generic;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation property
    public ICollection<Movie> Movies { get; set; }
}
