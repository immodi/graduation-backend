
using System.Text.Json.Serialization;
using backend.Models;

namespace backend.DTOs.Responses;

public record AuthResponse(
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("token")] string Token
): BaseResponse(200);


public record ResetResponse(
    [property: JsonPropertyName("email")] string Email
): BaseResponse(200);


public record AuthUpdateResponse(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email
): BaseResponse(200);

public record ResetData(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("code")] string Code
): BaseResponse(200);

public record UserInfoResponse(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email
): BaseResponse(200);

