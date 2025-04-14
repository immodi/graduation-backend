namespace backend.DTOs.Requests;
using System.Text.Json.Serialization;

public record AiRequest(
    [property: JsonPropertyName("message")] string Message
): BaseRequest;

public record GroqResponse(
    [property: JsonPropertyName("choices")]
    List<Choice> Choices
);

public record Choice(
    [property: JsonPropertyName("message")]
    Message Message
);

public record Message(
    [property: JsonPropertyName("content")]
    string Content
);
