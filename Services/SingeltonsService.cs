using Docker.DotNet;

namespace backend.Services;

public static class SingeltonsService
{
    private static readonly string DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "db.db");
    
    public static void AddCustomSingeltons(this IServiceCollection services)
    {
       
        services.AddSingleton<JwtService>();
        services.AddSingleton(new DatabaseService(DbPath));
        services.AddSingleton(new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient());
    }
}