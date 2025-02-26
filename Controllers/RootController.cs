namespace backend.Controllers;

public class RootController
{
    private readonly string[] _supportedLanguages = ["javascript", "csharp", "python"];
    public object Index()
    {
        return new
        {
            supportedLanguages = _supportedLanguages,
            date = DateTime.Now
        };
    }
}
