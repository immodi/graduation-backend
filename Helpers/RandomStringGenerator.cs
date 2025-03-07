namespace backend.Helpers;

public static class RandomStringGenerator
{
    private static readonly Random Random = new();
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString()
    {
        return new string(Enumerable.Range(0, 8).Select(_ => Chars[Random.Next(Chars.Length)]).ToArray());
    }
}