using backend.Helpers;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace backend.Services;

public class DockerAltService(DockerClient dockerClient)
{
    public async Task<string> StartContainerAsync(string language, string codeToRun)
    {
        if (!ProgrammingLanguages.Commands.TryGetValue(language, out var commandTemplate))
            throw new Exception("Unknown language: " + language);

        var commandArray = commandTemplate.Command(codeToRun);
        
        var containerConfig = new CreateContainerParameters
        {
            Image = commandTemplate.ImageName,
            Cmd = commandArray,
            AttachStdin = true,
            OpenStdin = true,
            Tty = false,
            HostConfig = new HostConfig
            {
                AutoRemove = false
            }
        };

        var response = await dockerClient.Containers.CreateContainerAsync(containerConfig);
        var containerId = response.ID;

        if (string.IsNullOrEmpty(containerId))
        {
            throw new Exception("Failed to create container for language: " + language);
        }

        var started = await dockerClient.Containers.StartContainerAsync(containerId, null);
        if (!started)
        {
            throw new Exception("Failed to start container: " + containerId);
        }

        return containerId;
    }

    public async Task RunCodeInRunningContainerAsync(string language, string codeToRun, string containerId)
    {
        if (!ProgrammingLanguages.Commands.TryGetValue(language, out var commandTemplate))
            throw new Exception("Unknown language: " + language);

        var commandArray = commandTemplate.Command(codeToRun);
        await SendInputToContainerAsync(containerId, string.Join(" ", commandArray));
    }

    public async Task<DockerStdOut> GetContainerStdoutAsync(string containerId)
    {
        // Get container logs (stdout)
        var logOptions = new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Tail = "all"
        };
    
        using var multiplexedStream = await dockerClient.Containers.GetContainerLogsAsync(containerId, false, logOptions);
   
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
            
        try
        {
            await multiplexedStream.CopyOutputToAsync(null, stdout, stderr, new CancellationToken(false));
        }  catch (OperationCanceledException)
        {
            await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        }
        
        stdout.Position = 0;
        stderr.Position = 0;

        using var stdoutReader = new StreamReader(stdout);
        using var stderrReader = new StreamReader(stderr);

        var stdoutOutput = await stdoutReader.ReadToEndAsync();
        var stderrOutput = await stderrReader.ReadToEndAsync();

        
        return new DockerStdOut(stdoutOutput, stderrOutput);
    }
    
    public async Task<bool> SendInputToContainerAsync(string containerId, string input)
    {
        try
        {
            // Ensure input ends with a newline
            if (!input.EndsWith('\n'))
            {
                input += "\n";
            }
        
            // Convert string to bytes
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        
            // Create a request to attach to the container
            var attachParameters = new ContainerAttachParameters
            {
                Stream = true,
                Stdin = true,
                Stdout = false,
                Stderr = false
            };
        
            // Attach to the container
            using var multiplexedStream = await dockerClient.Containers.AttachContainerAsync(
                containerId, 
                false, 
                attachParameters);
        
            // Write directly to the multiplexed stream's input
            await multiplexedStream.WriteAsync(
                inputBytes, 
                0, 
                inputBytes.Length, 
                CancellationToken.None);
        
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending input to container {containerId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> KillContainerAsync(string containerId)
    {
        try
        {
            // Verify the container exists
            try
            {
                await dockerClient.Containers.InspectContainerAsync(containerId);
            }
            catch (DockerContainerNotFoundException)
            {
                // Container doesn't exist, so nothing to do
                return true;
            }

            // First, try to kill the container (force stop)
            try
            {
                await dockerClient.Containers.KillContainerAsync(containerId, new ContainerKillParameters());
            }
            catch (DockerApiException ex)
            {
                // If container is already stopped, this might throw an exception
                // We'll log it but continue trying to remove the container
                Console.WriteLine($"Warning when killing container {containerId}: {ex.Message}");
            }

            // Then remove the container with force option
            await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
            {
                Force = true, // Force removal even if it's running
                RemoveVolumes = true // Remove associated volumes
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error killing/removing container {containerId}: {ex.Message}");
            return false;
        }
    }
}

public record DockerStdOut(string StdOut, string StdErr);
