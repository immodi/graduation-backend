using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record CompileResponse(
    [property: JsonPropertyName("output")] string Output,
    [property: JsonPropertyName("isSuccess")] bool IsSuccess
    ): BaseResponse(200);