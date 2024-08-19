using System.ComponentModel.DataAnnotations;

namespace cinema_web_app.Models;

public class CreateUserViewModel
{
    [Microsoft.Build.Framework.Required] public string FirstName { get; set; }

    [Microsoft.Build.Framework.Required] public string LastName { get; set; }

    [Microsoft.Build.Framework.Required]
    [EmailAddress]
    public string Email { get; set; }

    [Microsoft.Build.Framework.Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Microsoft.Build.Framework.Required] public string Role { get; set; }
}