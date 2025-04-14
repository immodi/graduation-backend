using Docker.DotNet;

namespace backend.Services;

public static class SingeltonsService
{
    private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "db.db");
    
    public static void AddCustomSingeltons(this IServiceCollection services)
    {
        services.AddSingleton<JwtService>();
        services.AddSingleton(new DatabaseService(DbPath));
        services.AddSingleton(
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient());

        var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(groqApiKey))
        {
            throw new InvalidOperationException("GROQ_API_KEY environment variable is not set.");
        }

        services.AddSingleton(new GroqService(groqApiKey));
    }
}