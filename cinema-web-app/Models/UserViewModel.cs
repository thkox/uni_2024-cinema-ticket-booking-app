namespace cinema_web_app.Models;

public class UserViewModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public IList<string> Roles { get; set; }
    public IList<string> CinemaNames { get; set; }
}