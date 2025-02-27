using System.Text.Json.Serialization;

namespace backend.DTOs.Requests;

public record RegisterRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password
): BaseRequest;