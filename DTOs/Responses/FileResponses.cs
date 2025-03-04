using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record FileReadResponse(
    [property: JsonPropertyName("filename")]
    string FileName,
    [property: JsonPropertyName("fileContent")]
    string FileContent,
    [property: JsonPropertyName("fileCreationDate")]
    string FileCreationDate,
    [property: JsonPropertyName("lastModifiedDate")]
    string LastModifiedDate,
    [property: JsonPropertyName("fileSizeInBytes")]
    int FileSizeInBytes
) : BaseResponse(200);


public record FileCreationResponse(
    [property: JsonPropertyName("fileId")] int FileId,
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("fileSizeInBytes")] int FileSizeInBytes
): BaseResponse(200);