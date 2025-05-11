using System.Net.WebSockets;
using System.Text;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace backend.Helpers
{
    public static class WebSocketHelpers
    {
        // Helper method to send a closing message and close the WebSocket connection
        private static async Task SendClosingMessageAsync(WebSocket webSocket)
        {
            const string closingMessage = "Closing connection...";
            var closingBuffer = Encoding.UTF8.GetBytes(closingMessage);

            // Send a close message to the client (optional, for clarity)
            await webSocket.SendAsync(new ArraySegment<byte>(closingBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // Helper method to close the WebSocket connection properly
        private static async Task CloseWebSocketAsync(WebSocket webSocket, DockerAltService dockerAltService, string containerId)
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
        private static bool TryParseMessage(string message, out WebSocketCompileRequest? compileRequest)
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

        // Helper method to process received WebSocket messages
        public static async Task ProcessWebSocketMessagesAsync(WebSocket webSocket, byte[] buffer, DockerAltService dockerAltService, string containerId = "")
        {
            var errorResponse = new ErrorResponse("invalid request body");
            var errorJson = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var request = TryParseMessage(message);

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
                            await CloseWebSocketAsync(webSocket, dockerAltService, containerId);
                            return;
                        
                        case WebSocketCompileRequestType.Code:
                            if (string.IsNullOrEmpty(containerId))
                            {
                                // Start a new Docker container if none is running
                                containerId = await dockerAltService.StartContainerAsync(request.Language, request.CodeToRun);
                                var stdOut = await dockerAltService.GetContainerStdoutAsync(containerId);

                                // Send stdout response to WebSocket
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(stdOut.StdOut)), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                // If a container is already running, just execute the existing code or perform an action
                                await dockerAltService.RunCodeInRunningContainerAsync(request.Language, request.CodeToRun, containerId);
                                var stdOut = await dockerAltService.GetContainerStdoutAsync(containerId);

                                // Send the existing stdout response to WebSocket (or handle accordingly)
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(stdOut.StdOut)), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            break;
                        
                        case WebSocketCompileRequestType.Command:
                            if (string.IsNullOrEmpty(containerId))
                            {
                                // Send an error message if no container is running
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Error: No container running")), WebSocketMessageType.Text, true, CancellationToken.None);
                                continue;
                            }

                            // Send input to the existing container
                            var isSent = await dockerAltService.SendInputToContainerAsync(containerId, request.CodeToRun);
                            if (isSent)
                            {
                                var responseMessage = await dockerAltService.GetContainerStdoutAsync(containerId);
                                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage.StdOut);

                                // Send the response from the container's stdout
                                await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                // Handle case when input is not sent successfully
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Error: Failed to send command to container")), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
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
                Console.WriteLine($"Error occurred: {ex.Message}");
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            finally
            {
                await CloseWebSocketAsync(webSocket, dockerAltService, containerId);
                Console.WriteLine("WebSocket connection closed.");
            }
        }

        private static WebSocketCompileRequest? TryParseMessage(string message)
        {
            if (!TryParseMessage(message, out var compileRequest))
            {
                return null;
            }

            return compileRequest;
        }
    }
}
