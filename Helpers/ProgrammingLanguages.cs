namespace backend.Helpers;

public static class ProgrammingLanguages
{
    public record LanguageConfig(string ImageName, Func<string, string[]> Command);

    public static readonly Dictionary<string, LanguageConfig> Commands = new()
    {
        { "python", new LanguageConfig("python:3-alpine3.21", s => ["python", "-c", s ]) },
        { "javascript", new LanguageConfig("node:22-alpine3.21", s => ["node", "-e", s ]) },
        {
            "csharp", new LanguageConfig(
                "mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim",
                s =>
                [
                    "bash", "-c",
                    "dotnet tool install -g dotnet-script > /dev/null 2>&1 && " +
                    "export PATH=\"$PATH:/root/.dotnet/tools\" && " +
                    "cat <<EOF > script.csx\n" + s + "\nEOF\n" +
                    "dotnet-script script.csx"
                ]
            )
        },
        {
            "java", new LanguageConfig(
                "openjdk:24-jdk-slim-bookworm",
                s =>
                [
                    "bash", "-c",
                    $"cat <<EOF > Main.java\n{s}\nEOF\n" +
                    "javac Main.java && java Main"
                ]
            )
        }


    };
}