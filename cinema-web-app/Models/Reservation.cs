namespace cinema_web_app.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid ScreeningId { get; set; }
    public Guid CustomerId { get; set; }
    public int NoOfBookedSeats { get; set; }

    // Navigation properties
    [ForeignKey("ScreeningId")]
    public Screening Screening { get; set; }

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }
}
