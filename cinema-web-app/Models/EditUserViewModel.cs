using System.ComponentModel.DataAnnotations;

namespace cinema_web_app.Models;

public class EditUserViewModel
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "The First Name field can only contain letters.")]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "The Last Name field can only contain letters.")]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Cinema")] public Guid? CinemaId { get; set; } // Add property for selected cinema ID

    public bool IsContentCinemaAdmin { get; set; } // Add property to check if user is ContentCinemaAdmin
}