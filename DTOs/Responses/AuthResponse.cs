
using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record AuthResponse(
    [property: JsonPropertyName("token")] string Token
): BaseResponse(200);