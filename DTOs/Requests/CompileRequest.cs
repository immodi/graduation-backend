namespace backend.DTOs.Requests;

using System.Text.Json.Serialization;

public record CompileRequest(
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("codeToRun")] string CodeToRun
): BaseRequest;
