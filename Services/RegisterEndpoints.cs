using backend.Controllers;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using Docker.DotNet;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services;

public static class WebApplicationExtension
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.Map("/", () => new RootController().Index().ToResult());
        app.MapPost("/register", (JwtService jwtService, [FromBody] string username) => new RegisterController(jwtService, username).RegisterUser().ToResult());
        app.MapPost("/compile", async (DockerClient dockerClient, [FromBody] CompileRequest compileRequest) =>
        (await new CompileController(dockerClient, compileRequest).Compile()).ToResult()).RequireAuthorization();
        
        app.MapFallback(() => Results.NotFound(new ErrorResponse("Endpoint or Method not found")));
    }
}