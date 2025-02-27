using Docker.DotNet;

namespace backend.Services;

public static class SingeltonsService
{
    public static void AddCustomSingeltons(this IServiceCollection services)
    {
        services.AddSingleton<JwtService>();
        services.AddSingleton(new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient());
    }
}