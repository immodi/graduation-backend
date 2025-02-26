using backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services;

public static class WebApplicationExtension
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.Map("/", () => new RootController().Index()).RequireAuthorization();
        app.MapPost("/register", (JwtService jwtService, [FromBody] string username) => new RegisterController(jwtService, username).RegisterUser());
        
        app.MapFallback(() => Results.NotFound(new { message = "Endpoint or Method not found" }));
    }
}