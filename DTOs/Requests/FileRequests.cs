using System.Text.Json.Serialization;

namespace backend.DTOs.Requests;


public record FileReadRequest(
    [property: JsonPropertyName("fileId")] int FileId
): BaseRequest;


public record FileCreationRequest(
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("fileContent")] string FileContent
): BaseRequest;


public record FileUpdateRequest(
    [property: JsonPropertyName("fileId")] int FileId,
    [property: JsonPropertyName("newFileName")] string NewFileName,
    [property: JsonPropertyName("newFileContent")] string NewFileContent
): BaseRequest;

public record FileDeleteRequest(
    int FileId
): FileReadRequest(FileId);

public record FileShareRequest(
    int FileId
): FileReadRequest(FileId);


public record FileShareReadRequest(
    [property: JsonPropertyName("fileShareUrl")] string FileShareLink
): BaseRequest;