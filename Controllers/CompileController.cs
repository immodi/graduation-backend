using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;
using Docker.DotNet;

namespace backend.Controllers;

public class CompileController(DockerClient dockerClient)
{
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
}