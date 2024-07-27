namespace cinema_web_app.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class ScreeningRoom
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; }
    public int TotalNoOfSeats { get; set; }
    public bool Is3D { get; set; }

    // Navigation properties
    [ForeignKey("CinemaId")]
    public Cinema Cinema { get; set; }
    public ICollection<Screening> Screenings { get; set; }
}