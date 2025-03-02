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
        app.Map("/doc", async (context) =>
        {
            await context.Response.SendFileAsync("Static/reference.html");
        });        
        
        app.MapPost("/login", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest registerRequest) =>
        (await new LoginController(jwtService, databaseService, registerRequest).LoginUser()).ToResult());
        app.MapPost("/register", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest registerRequest) =>
        (await new RegisterController(jwtService, databaseService, registerRequest).RegisterUser()).ToResult());
        app.MapPost("/compile", async (DockerClient dockerClient, [FromBody] CompileRequest compileRequest) =>
        (await new CompileController(dockerClient, compileRequest).Compile()).ToResult()).RequireAuthorization();
        
        app.MapFallback(() => new ErrorResponse("Endpoint or Method not found"){StatusCode = 404}.ToResult());
    }
}