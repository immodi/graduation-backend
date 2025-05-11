using System.Net.WebSockets;
using System.Reflection;
using System.Text;
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
        app.MapPost("/request-password-reset", async (JwtService jwtService, DatabaseService databaseService, [FromBody] AuthResetRequest authResetRequest) =>
        (await new AuthController(jwtService, databaseService).RequestResetPassword(authResetRequest)).ToResult());
        app.MapPost("/reset-password", async (JwtService jwtService, DatabaseService databaseService, [FromBody] ResetRequest resetRequest) => 
        (await new AuthController(jwtService, databaseService).ResetPassword(resetRequest)).ToResult());
        app.MapPost("/user/update", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] AuthUpdateRequest authUpdateRequest) => 
        (await new AuthController(jwtService, databaseService).AuthUpdate(userToken: httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim(), request: authUpdateRequest)).ToResult());
        app.MapGet("/user/info", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService) => 
        (await new AuthController(jwtService, databaseService).GetUserInfo(userToken: httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim())).ToResult()).RequireAuthorization();

        
        app.MapPost("/compile", async (DockerClient dockerClient, [FromBody] CompileRequest compileRequest) =>
        (await new CompileController(dockerClient).Compile(compileRequest)).ToResult()).RequireAuthorization();
        
        app.MapGet("/file/all", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService) =>
        (await new FileController(httpContext, jwtService, databaseService).GetAllFiles()).ToResult()).RequireAuthorization();
        app.MapGet("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService) =>
        (await new FileController(httpContext, jwtService, databaseService).ReadFile()).ToResult()).RequireAuthorization();
        app.MapPost("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileCreationRequest fileCreationRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).CreateFile(fileCreationRequest)).ToResult()).RequireAuthorization();
        app.MapPatch("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileUpdateRequest fileUpdateRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).UpdateFile(fileUpdateRequest)).ToResult()).RequireAuthorization();
        app.MapDelete("/file", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileDeleteRequest fileDeleteRequest) =>
        (await new FileController(httpContext, jwtService, databaseService).DeleteFile(fileDeleteRequest)).ToResult()).RequireAuthorization();

        
        app.MapGet("/share", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService) =>
        (await new ShareController(httpContext, jwtService, databaseService).ReadSharedFile()).ToResult());
        app.MapPatch("/share", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService,  [FromBody] SharedFileUpdateRequest sharedFileUpdateRequest) =>
            (await new ShareController(httpContext, jwtService, databaseService).UpdateSharedFile(sharedFileUpdateRequest)).ToResult());
        app.MapPost("/share", async (HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, [FromBody] FileShareRequest fileShareRequest) =>
        (await new ShareController(httpContext, jwtService, databaseService).ShareFile( userToken: httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim(), request: fileShareRequest) ).ToResult()).RequireAuthorization();
        
        app.MapPost("/ai", async (AiService groqService, [FromBody] AiRequest aiRequest) =>
        (await new AiController(groqService).ChatWithTheAi(aiRequest)).ToResult());
        app.MapGet("/ai/models", (AiService groqService) => new AiController(groqService).GetAllAiModels());




        // Handle /compile WebSocket connections
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/compile")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var dockerClient = context.RequestServices.GetRequiredService<DockerClient>();
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await new CompileController(dockerClient).HandleWebSocketAsync(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        });
        
        app.MapFallback(() => new ErrorResponse("Endpoint or Method not found"){StatusCode = 404}.ToResult());
    }
}
