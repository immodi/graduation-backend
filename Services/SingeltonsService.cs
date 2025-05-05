using backend.Contexts;
using backend.Interfaces;
using backend.Repositories;
using Docker.DotNet;

namespace backend.Services;

public static class SingeltonsService
{
    private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "db.db");
    
    public static void AddCustomSingeltons(this IServiceCollection services)
    {
        services.AddSingleton(new DbContext(DbPath));
        services.AddSingleton<IUserRepository, UserRepository>(provider =>
        {
            var dbContext = provider.GetRequiredService<DbContext>();
            
            return new UserRepository(dbContext.Database);
        });
        
        services.AddSingleton<IFileRepository, FileRepository>(provider =>
        {
            var dbContext = provider.GetRequiredService<DbContext>();
            
            return new FileRepository(dbContext.Database);
        });

        services.AddSingleton<JwtService>();

        services.AddSingleton<DatabaseService>(provider =>
        {
            var userRepository = provider.GetRequiredService<IUserRepository>();
            var fileRepository = provider.GetRequiredService<IFileRepository>();
            
            return new DatabaseService(userRepository, fileRepository);
        });

        services.AddSingleton(
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient());

        var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrWhiteSpace(groqApiKey))
        {
            throw new InvalidOperationException("GROQ_API_KEY environment variable is not set.");
        }

        services.AddSingleton(new AiService(groqApiKey));
    }
}