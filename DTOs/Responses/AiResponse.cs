using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record AiResponse(
    [property: JsonPropertyName("response")] string Response
): BaseResponse(200);

public record AiModelsResponse(
    [property: JsonPropertyName("allModels")] string[] Models
): BaseResponse(200);