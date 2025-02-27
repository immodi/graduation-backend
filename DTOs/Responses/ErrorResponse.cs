namespace backend.DTOs.Responses;

using System.Text.Json.Serialization;

public record ErrorResponse(
    [property: JsonPropertyName("errorMessage")] string ErrorMessage
    
): BaseResponse(400);
