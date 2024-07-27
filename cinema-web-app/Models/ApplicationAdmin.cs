namespace cinema_web_app.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class ApplicationAdmin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
}