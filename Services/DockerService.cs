using Docker.DotNet;
using Docker.DotNet.Models;
using backend.DTOs.Responses;
using backend.Helpers;

namespace backend.Services;

public class DockerService(DockerClient dockerClient, string language, string codeToRun, int timeoutSeconds = 30)
{

    public async Task<BaseResponse> Run()
    {
        if (!ProgrammingLanguages.Commands.TryGetValue(language, out var commandTemplate))
            return new ErrorResponse("There was an error running the programming language.");

        var commandArray = commandTemplate.Command(codeToRun);

        var containerConfig = new CreateContainerParameters
        {
            Image = commandTemplate.ImageName,
            Cmd = commandArray,
            HostConfig = new HostConfig { AutoRemove = false }
        };

        var response = await dockerClient.Containers.CreateContainerAsync(containerConfig);
        var containerId = response.ID;

        try
        {
            await dockerClient.Containers.StartContainerAsync(containerId, null);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            using var logsStream = await dockerClient.Containers.GetContainerLogsAsync(containerId, false,
                new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Follow = true
                }, cts.Token);

            using var stdout = new MemoryStream();
            using var stderr = new MemoryStream();
            
            try
            {
                await logsStream.CopyOutputToAsync(null, stdout, stderr, cts.Token);
            }
            catch (OperationCanceledException)
            {
                await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
            }

            stdout.Position = 0;
            stderr.Position = 0;

            using var stdoutReader = new StreamReader(stdout);
            using var stderrReader = new StreamReader(stderr);

            var stdoutOutput = await stdoutReader.ReadToEndAsync();
            var stderrOutput = await stderrReader.ReadToEndAsync();

            var combinedOutput = $"{stdoutOutput.Trim()}\n{stderrOutput.Trim()}".Trim();
            var output = string.IsNullOrEmpty(combinedOutput) ? "No output was generated." : combinedOutput;

            return new CompileResponse(output, string.IsNullOrEmpty(stderrOutput));
        }
        catch (Exception ex)
        {
            return new ErrorResponse($"Docker execution error: {ex.Message}");
        }
        finally
        {
            try
            {
                await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { Force = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to remove container {containerId}: {ex.Message}");
            }
        }
    }
}
