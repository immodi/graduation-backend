using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public abstract record BaseResponse(
    [property: JsonPropertyName("dateTime")] string DateTime,
    [property: JsonPropertyName("statusCode")] int StatusCode
)
{
    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    protected BaseResponse(int statusCode) 
        : this(System.DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"), statusCode) {}

    public IResult ToResult() => Results.Json(this, SerializationOptions, statusCode: StatusCode);}


