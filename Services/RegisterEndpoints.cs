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
        
        app.MapPost("/login", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest loginRequest) =>
        (await new AuthController(jwtService, databaseService).LoginUser(loginRequest)).ToResult());
        app.MapPost("/register", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthRequest registerRequest) =>
        (await new AuthController(jwtService, databaseService).RegisterUser(registerRequest)).ToResult());
        
        app.MapPost("/compile", async (DockerClient dockerClient, [FromBody] CompileRequest compileRequest) =>
        (await new CompileController(dockerClient).Compile(compileRequest)).ToResult()).RequireAuthorization();
        
        app.MapGet("/file/all", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] AllFilesRequest allFilesRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).GetAllFiles(allFilesRequest)).ToResult()).RequireAuthorization();
        app.MapGet("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileReadRequest fileReadRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).ReadFile(fileReadRequest)).ToResult()).RequireAuthorization();
        app.MapPost("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileCreationRequest fileCreationRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).CreateFile(fileCreationRequest)).ToResult()).RequireAuthorization();
        app.MapPatch("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileUpdateRequest fileUpdateRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).UpdateFile(fileUpdateRequest)).ToResult()).RequireAuthorization();
        app.MapDelete("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileDeleteRequest fileDeleteRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).DeleteFile(fileDeleteRequest)).ToResult()).RequireAuthorization();

        
        app.MapPost("/share", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileShareRequest fileShareRequest) =>
        (await new ShareController(httpContext, jwtService, databaseService).ShareFile( userToken: httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim(), request: fileShareRequest) ).ToResult()).RequireAuthorization();
        app.MapGet("/share", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileShareReadRequest fileShareReadRequest) =>
        (await new ShareController(httpContext, jwtService, databaseService).ReadSharedFile(fileShareReadRequest)).ToResult());
        
        app.MapFallback(() => new ErrorResponse("Endpoint or Method not found"){StatusCode = 404}.ToResult());
    }
}