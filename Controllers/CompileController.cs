using System.Net.WebSockets;
using System.Text;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Helpers;
using backend.Services;
using Docker.DotNet;

namespace backend.Controllers;

public class CompileController(DockerClient dockerClient)
{
    private readonly DockerAltService _dockerAltService = new(dockerClient);
    
    public async Task<BaseResponse> Compile(CompileRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            var output = await new DockerService(dockerClient, request.Language, request.CodeToRun).Run();
            return output;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var process = new ProcessingWebSocketsClass();
        await process.ProcessWebSocketMessagesAsync(webSocket, buffer, _dockerAltService, dockerClient);
    }
}