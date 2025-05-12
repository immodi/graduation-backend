using System.Text;
using backend.Helpers;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace backend.Services;

public record DockerContainerOutput(
    string ContainerId, MultiplexedStream? AttachedStream);

public class DockerAltService(DockerClient dockerClient)
{
    public async Task<DockerContainerOutput> StartContainerAsync(string language, string codeToRun)
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

        var attachParams = new ContainerAttachParameters
        {
            Stream = true,
            Stdout = true,
            Stderr = true
        };

        // Create the container
        var response = await dockerClient.Containers.CreateContainerAsync(containerConfig);
        var containerId = response.ID;

        if (string.IsNullOrEmpty(containerId))
        {
            throw new Exception("Failed to create container for language: " + language);
        }

        // Attach to the container's output stream
        var attachedStream = await dockerClient.Containers.AttachContainerAsync(containerId, false, attachParams);

        // Start the container
        var started = await dockerClient.Containers.StartContainerAsync(containerId, null);
        if (!started)
        {
            throw new Exception("Failed to start container: " + containerId);
        }
        
        // Infinite loop to check for output on stdout
        var initialOutput = new StringBuilder(); // Use StringBuilder to accumulate output
        var buffer = new byte[4096];

        while (true)
        {
            // Read the output
            var bytesRead = await attachedStream.ReadOutputAsync(buffer, 0, buffer.Length, CancellationToken.None);

            // If data is received, process it
            if (bytesRead.Count > 0)
            {
                var output = Encoding.UTF8.GetString(buffer, 0, bytesRead.Count).Trim();
        
                if (!string.IsNullOrEmpty(output))
                {
                    initialOutput.Append(output); // Accumulate the output

                    // Break the loop if any output is captured
                    break;
                }
            }

            // Optionally, add a small delay to prevent maxing out CPU usage
            await Task.Delay(100);
        }
                
        // Return the container ID, output stream, and initial output
        return new DockerContainerOutput(containerId, attachedStream);
    }


    public async Task RunCodeInRunningContainerAsync(string language, string codeToRun, string containerId)
    {
        if (!ProgrammingLanguages.Commands.TryGetValue(language, out var commandTemplate))
            throw new Exception("Unknown language: " + language);

        var commandArray = commandTemplate.Command(codeToRun);
        await SendInputToContainerAsync(containerId, string.Join(" ", commandArray));
    }

    public async Task<DockerStdOut> GetContainerLogsAsync(string containerId)
    {
        // Set up parameters for fetching logs
        var logParams = new ContainerLogsParameters
        {
            ShowStdout = true,  // Capture stdout
            ShowStderr = true,  // Capture stderr
            Tail = "all",       // Get all logs (or specify a number for recent logs)
            Follow = false      // Don't continuously stream logs, just grab them once
        };

        // Fetch the logs from the container
        var logs = await dockerClient.Containers.GetContainerLogsAsync(containerId, logParams);

        // Convert the logs into a string and return
        using var reader = new StreamReader(logs);
        var logOutput = await reader.ReadToEndAsync();

        // Return the logs (stdout + stderr)
        return new DockerStdOut(logOutput, string.Empty); // stderr is empty if not fetched
    }

    // public async Task<DockerStdOut> GetContainerStdoutAsync(MultiplexedStream attachedStream)
    // {
    //     using var stdout = new MemoryStream();
    //     using var stderr = new MemoryStream();
    //
    //     var buffer = new byte[4096];
    //     var cancellationSource = new CancellationTokenSource();
    //     cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // short read timeout
    //
    //     try
    //     {
    //         while (true)
    //         {
    //             var bytesRead = await attachedStream.ReadOutputAsync(buffer, 0, buffer.Length, cancellationSource.Token);
    //             if (bytesRead.Count <= 0) break;
    //
    //             stdout.Write(buffer, 0, bytesRead.Count);
    //         }
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         // Done reading what's available
    //     }
    //
    //     stdout.Position = 0;
    //     stderr.Position = 0;
    //
    //     using var stdoutReader = new StreamReader(stdout);
    //     using var stderrReader = new StreamReader(stderr);
    //
    //     var stdoutOutput = await stdoutReader.ReadToEndAsync();
    //     var stderrOutput = await stderrReader.ReadToEndAsync();
    //
    //     return new DockerStdOut(stdoutOutput, stderrOutput);
    // }
    //
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
