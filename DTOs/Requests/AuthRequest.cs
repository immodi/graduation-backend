using System.Text.Json.Serialization;

namespace backend.DTOs.Requests;

public record AuthRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
): BaseRequest;

public record AuthResetRequest(
    [property: JsonPropertyName("username")] string Username
): BaseRequest;

public record AuthUpdateRequest(
    [property: JsonPropertyName("newUsername")] string? Username,
    [property: JsonPropertyName("newEmail")] string? Email
): BaseRequest;

public record ResetRequest(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("newPassword")] string NewPassword
): BaseRequest;