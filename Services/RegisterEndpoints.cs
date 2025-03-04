using System.Reflection;
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
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.Static.reference.html";
            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("File not found.");
                return;
            }
            context.Response.ContentType = "text/html";
            await stream.CopyToAsync(context.Response.Body);
        });       
        
        app.MapPost("/login", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest registerRequest) =>
        (await new AuthController(jwtService, databaseService, registerRequest).LoginUser()).ToResult());
        app.MapPost("/register", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest registerRequest) =>
        (await new AuthController(jwtService, databaseService, registerRequest).RegisterUser()).ToResult());
        
        app.MapPost("/compile", async (DockerClient dockerClient, [FromBody] CompileRequest compileRequest) =>
        (await new CompileController(dockerClient, compileRequest).Compile()).ToResult()).RequireAuthorization();
        
        app.MapGet("/file", async (DatabaseService databaseService, [FromBody] FileReadRequest fileReadRequest) =>
        (await new FileController(databaseService, fileReadRequest).ReadFile()).ToResult()).RequireAuthorization();
        app.MapPost("/file", async (DatabaseService databaseService, [FromBody] FileCreationRequest fileCreationRequest) =>
        (await new FileController(databaseService, fileCreationRequest).CreateFile()).ToResult()).RequireAuthorization();
        app.MapPatch("/file", async (DatabaseService databaseService, [FromBody] FileUpdateRequest fileUpdateRequest) =>
        (await new FileController(databaseService, fileUpdateRequest).UpdateFile()).ToResult()).RequireAuthorization();

        
        app.MapFallback(() => new ErrorResponse("Endpoint or Method not found"){StatusCode = 404}.ToResult());
    }
}