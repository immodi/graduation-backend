using System.Net.WebSockets;
using System.Text;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;
using Docker.DotNet;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace backend.Helpers
{
    public static class WebSocketHelpers
    {
        // Helper method to send a closing message and close the WebSocket connection
        public static async Task SendClosingMessageAsync(WebSocket webSocket)
        {
            const string closingMessage = "Closing connection...";
            var closingBuffer = Encoding.UTF8.GetBytes(closingMessage);

            // Send a close message to the client (optional, for clarity)
            await webSocket.SendAsync(new ArraySegment<byte>(closingBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // Helper method to close the WebSocket connection properly
        public static async Task CloseWebSocketAsync(WebSocket webSocket, DockerAltService dockerAltService, string containerId)
        {
            try
            {
                Console.WriteLine($"contianer id + {containerId}");
                await dockerAltService.KillContainerAsync(containerId);
                await SendClosingMessageAsync(webSocket);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server is closing the connection.", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing WebSocket connection: {ex.Message}");
            }
        }

        // Helper method to parse the first message into CompileRequest
        public static bool TryParseMessage(string message, out WebSocketCompileRequest? compileRequest)
        {
            compileRequest = null;
            try
            {
                compileRequest = JsonSerializer.Deserialize<WebSocketCompileRequest>(message);
                return compileRequest != null;
            }
            catch (JsonException ex)
            {
                return false;
            }
        }
        public static WebSocketCompileRequest? TryParseMessage(string message)
        {
            if (!TryParseMessage(message, out var compileRequest))
            {
                return null;
            }

            return compileRequest;
        }
    }

    public class ProcessingWebSocketsClass
    {
        // public async Task ProcessWebSocketMessagesAsync(WebSocket webSocket, byte[] buffer, DockerAltService dockerAltService, DockerClient dockerClient, DockerContainerOutput containerData = null)
        // {
        //     var errorResponse = new ErrorResponse("invalid request body");
        //     var errorJson = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
        //     {
        //         ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        //     });
        //     
        //     var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //
        //     while (!result.CloseStatus.HasValue)
        //     {
        //         var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        //         var request = WebSocketHelpers.TryParseMessage(message);
        //
        //         if (request is null)
        //         {
        //             await webSocket.SendAsync(
        //                 new ArraySegment<byte>(Encoding.UTF8.GetBytes(errorJson)),
        //                 WebSocketMessageType.Text,
        //                 true,
        //                 CancellationToken.None);
        //             result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //             continue;
        //         }
        //
        //         switch (request.Type)
        //         {
        //             case WebSocketCompileRequestType.Exit:
        //                 await WebSocketHelpers.CloseWebSocketAsync(webSocket, dockerAltService, containerData.ContainerId);
        //                 return;
        //             
        //             case WebSocketCompileRequestType.Code:
        //                 if (containerData is null || !await IsContainerRunningAsync(containerData.ContainerId, dockerClient))
        //                 {
        //                     // Start a new Docker container if none is running or the existing one isn't active
        //                     containerData = await dockerAltService.StartContainerAsync(request.Language, request.CodeToRun);
        //                     // var stdResp = await dockerAltService.GetContainerStdoutAsync(containerData.AttachedStream!);
        //                     var stdResp = await dockerAltService.GetContainerLogsAsync(containerData.ContainerId);
        //
        //                     var stdOut = $"{stdResp.StdOut} {stdResp.StdErr}";
        //
        //                     // Send stdout response to WebSocket
        //                     // await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(stdOut)), WebSocketMessageType.Text, true, CancellationToken.None);
        //                 }
        //                 break;
        //             
        //             case WebSocketCompileRequestType.Command:
        //                 if (string.IsNullOrEmpty(containerData.ContainerId))
        //                 {
        //                     // Send an error message if no container is running
        //                     // await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Error: No container running")), WebSocketMessageType.Text, true, CancellationToken.None);
        //                     continue;
        //                 }
        //
        //                 // Send input to the existing container
        //                 var isSent = await dockerAltService.SendInputToContainerAsync(containerData.ContainerId, request.CodeToRun);
        //                 if (isSent)
        //                 {
        //                     // var responseMessage = await dockerAltService.GetContainerStdoutAsync(containerData.AttachedStream);
        //                     var responseMessage = await dockerAltService.GetContainerLogsAsync(containerData.ContainerId);
        //                     var responseBuffer = Encoding.UTF8.GetBytes(responseMessage.StdOut);
        //
        //                     // Send the response from the container's stdout
        //                     // await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        //                 }
        //                 else
        //                 {
        //                     // Handle case when input is not sent successfully
        //                     // await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Error: Failed to send command to container")), WebSocketMessageType.Text, true, CancellationToken.None);
        //                 }
        //                 break;
        //             
        //             default:
        //                 await webSocket.SendAsync(
        //                     new ArraySegment<byte>(Encoding.UTF8.GetBytes(errorJson)),
        //                     WebSocketMessageType.Text,
        //                     true,
        //                     CancellationToken.None);
        //                 break;
        //         }
        //         
        //         // Receive the next WebSocket message (if any)
        //         result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //     }
        //     // try
        //     // {
        //     //     
        //     // }
        //     // catch (Exception ex)
        //     // {
        //     //     Console.WriteLine($"Error occurred: {ex.Message}");
        //     //     await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), WebSocketMessageType.Text, true, CancellationToken.None);
        //     // }
        //     // finally
        //     // {
        //     //     await WebSocketHelpers.CloseWebSocketAsync(webSocket, dockerAltService, containerData.ContainerId);
        //     //     Console.WriteLine("WebSocket connection closed.");
        //     // }
        // }

        public async Task ProcessWebSocketMessagesAsync(WebSocket webSocket, byte[] buffer, DockerAltService dockerAltService, DockerClient dockerClient, DockerContainerOutput containerData = null)
{
    var errorResponse = new ErrorResponse("invalid request body");
    var errorJson = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
    });
    
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    // Create a cancellation token source for stopping the stream
    var streamCancellationTokenSource = new CancellationTokenSource();

    // Start a background task to continuously read and send stream output
    Task streamTask = null;

    // Keep track of the last sent logs to avoid resending
    string lastSentLogs = string.Empty;

    try 
    {
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var request = WebSocketHelpers.TryParseMessage(message);

            if (request is null)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(errorJson)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                continue;
            }

            switch (request.Type)
            {
                case WebSocketCompileRequestType.Exit:
                    // Cancel the stream task and close the WebSocket
                    streamCancellationTokenSource.Cancel();
                    if (streamTask != null)
                    {
                        await streamTask; // Wait for the stream task to complete
                    }
                    await WebSocketHelpers.CloseWebSocketAsync(webSocket, dockerAltService, containerData?.ContainerId);
                    return;
                
                case WebSocketCompileRequestType.Code:
                    // Stop any existing stream task
                    if (streamTask != null)
                    {
                        streamCancellationTokenSource.Cancel();
                        await streamTask;
                        streamCancellationTokenSource = new CancellationTokenSource();
                    }

                    // Reset last sent logs
                    lastSentLogs = string.Empty;

                    // Start a new container or use existing one
                    if (containerData is null || !await IsContainerRunningAsync(containerData.ContainerId, dockerClient))
                    {
                        containerData = await dockerAltService.StartContainerAsync(request.Language, request.CodeToRun);
                    }

                    // Start a new stream task to continuously read container output
                    streamTask = StartContainerStreamAsync(webSocket, dockerAltService, containerData, 
                        streamCancellationTokenSource.Token, lastSentLogs);
                    break;
                
                case WebSocketCompileRequestType.Command:
                    if (string.IsNullOrEmpty(containerData?.ContainerId))
                    {
                        continue;
                    }

                    // Send input to the existing container
                    var isSent = await dockerAltService.SendInputToContainerAsync(containerData.ContainerId, request.CodeToRun);
                    break;
                
                default:
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(errorJson)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    break;
            }
            
            // Receive the next WebSocket message (if any)
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
    }
    catch (Exception ex)
    {
        // Handle any exceptions that might occur during WebSocket processing
        Console.WriteLine($"Error occurred: {ex.Message}");
        try 
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), 
                WebSocketMessageType.Text, 
                true, 
                CancellationToken.None
            );
        }
        catch 
        {
            // Ignore any errors in sending the error message
        }
    }
    finally
    {
        // Ensure stream task is cancelled and WebSocket is closed
        streamCancellationTokenSource.Cancel();
        if (streamTask != null)
        {
            await streamTask;
        }
        await WebSocketHelpers.CloseWebSocketAsync(webSocket, dockerAltService, containerData?.ContainerId);
    }
}

// New method to continuously stream container output only when logs change
private async Task StartContainerStreamAsync(WebSocket webSocket, DockerAltService dockerAltService, 
    DockerContainerOutput containerData, CancellationToken cancellationToken, string lastSentLogs)
{
    try 
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Get the latest logs from the container
            var responseMessage = await dockerAltService.GetContainerLogsAsync(containerData.ContainerId);
            
            // Combine stdout and stderr
            var combinedOutput = $"{responseMessage.StdOut} {responseMessage.StdErr}".Trim();

            // Only send if there are new logs
            if (!string.IsNullOrEmpty(combinedOutput) && combinedOutput != lastSentLogs)
            {
                var responseBuffer = Encoding.UTF8.GetBytes(combinedOutput);

                // Send the output to the WebSocket
                await webSocket.SendAsync(
                    new ArraySegment<byte>(responseBuffer), 
                    WebSocketMessageType.Text, 
                    true, 
                    cancellationToken
                );

                // Update the last sent logs
                lastSentLogs = combinedOutput;
            }

            // Add a small delay to prevent tight looping
            await Task.Delay(100, cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
        // Normal cancellation, do nothing
    }
    catch (Exception ex)
    {
        // Log any unexpected errors
        Console.WriteLine($"Stream error: {ex.Message}");
    }
}
        public async Task<bool> IsContainerRunningAsync(string containerId, DockerClient dockerClient)
        {
            var container = await dockerClient.Containers.InspectContainerAsync(containerId);
            return container.State.Running;
        }
    }
}
