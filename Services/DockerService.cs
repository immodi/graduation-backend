using Docker.DotNet;
using Docker.DotNet.Models;
using backend.DTOs.Responses;
using backend.Helpers;

namespace backend.Services;

public class DockerService(DockerClient dockerClient, string language, string codeToRun)
{

    public async Task<BaseResponse> Run()
    {
        if (ProgrammingLanguages.Commands.TryGetValue(language, out var commandTemplate))
        {
            var commandArray = commandTemplate.Command(codeToRun);

            var containerConfig = new CreateContainerParameters
            {
                Image = commandTemplate.ImageName,
                Cmd = commandArray,
                HostConfig = new HostConfig { AutoRemove = true }
            };

            var response = await dockerClient.Containers.CreateContainerAsync(containerConfig);
            var containerId = response.ID;

            await dockerClient.Containers.StartContainerAsync(containerId, null);

            using var logsStream = await dockerClient.Containers.GetContainerLogsAsync(containerId, false,
                new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Follow = true
                }, CancellationToken.None);

            using var stdout = new MemoryStream();
            using var stderr = new MemoryStream();

            await logsStream.CopyOutputToAsync(null, stdout, stderr, CancellationToken.None);

            stdout.Position = 0;
            stderr.Position = 0;

            using var stdoutReader = new StreamReader(stdout);
            using var stderrReader = new StreamReader(stderr);

            var stdoutOutput = await stdoutReader.ReadToEndAsync();
            var stderrOutput = await stderrReader.ReadToEndAsync();

            // Combine both outputs to ensure all logs are captured
            var combinedOutput = $"{stdoutOutput.Trim()}\n{stderrOutput.Trim()}".Trim();
            var output = string.IsNullOrEmpty(combinedOutput) ? "No output was generated." : combinedOutput;
            
            return new CompileResponse(output);
        }
        else
        {
            return new ErrorResponse("There was an error running the programming language.");
        }
    }
}