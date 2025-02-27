namespace backend.Services;

public static class HandleCorsClass
{
    public static IServiceCollection HandleCors(this IServiceCollection services)
    {
        return services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()  
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });   
    }
}