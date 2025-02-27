namespace backend.Helpers;

public static class ProgrammingLanguages
{
    public record LanguageConfig(string ImageName, string[] Command);

    public static readonly Dictionary<string, LanguageConfig> Commands = new()
    {
        { "python", new LanguageConfig("python:3-alpine3.21", [ "python", "-c" ]) },
        { "javascript", new LanguageConfig("node:22-alpine3.21", ["node", "-e" ]) },
        // { "csharp", new LanguageConfig("mcr.microsoft.com/dotnet/sdk:8.0", [ "dotnet", "script", "-e" ]) }
    };
    
    

}