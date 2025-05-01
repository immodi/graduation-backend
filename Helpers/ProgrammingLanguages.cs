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
                    "mkdir -p /app && " +
                    "dotnet new console -o /app --no-restore --force > /dev/null && " +
                    "echo '" + s.Replace("'", "'\\''") + "' > /app/Program.cs && " +
                    "cd /app && " +
                    "(dotnet build -v q 2>&1 > /dev/null || true) && " +  // suppress build logs, ignore errors here
                    "dotnet run --no-build --project /app 2>&1"
                ]
            )
        },
        {
            "cpp", new LanguageConfig(
                "frolvlad/alpine-gxx:latest",
                s =>
                [
                    "sh", "-c",
                    "cat <<EOF > main.cpp\n" + s + "\nEOF\n" +
                    "g++ -std=c++17 main.cpp -o program && ./program"
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