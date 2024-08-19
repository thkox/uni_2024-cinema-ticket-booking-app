namespace cinema_web_app.Utilities;

public static class ReferenceIdGenerator
{
    private static readonly Random Random = new();
    private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateReferenceId(int length = 6)
    {
        return new string(Enumerable.Repeat(Characters, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}