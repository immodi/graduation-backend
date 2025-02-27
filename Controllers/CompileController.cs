using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;
using Docker.DotNet;

namespace backend.Controllers;

public class CompileController(DockerClient dockerClient, CompileRequest compileRequest)
{
    public async Task<BaseResponse> Compile()
    {
        var output = await new DockerService(dockerClient, compileRequest.Language, compileRequest.CodeToRun).Run();
        return output;
    }
}