using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record LanguagesResponse(
    [property: JsonPropertyName("supportedLanguages")] string[] SupportedLanguages
    ): BaseResponse(200);