namespace backend.DTOs.Requests;

using System.Text.Json.Serialization;

public record CompileRequest(
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("codeToRun")] string CodeToRun
): BaseRequest;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebSocketCompileRequestType
{
    Code,
    Command,
    Exit
}

public record WebSocketCompileRequest(
    [property: JsonPropertyName("type")] WebSocketCompileRequestType Type,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("codeToRun")] string CodeToRun
): BaseRequest;
