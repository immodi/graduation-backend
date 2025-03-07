using System.Text.Json.Serialization;

namespace backend.DTOs.Responses;

public record FileReadResponse(
    [property: JsonPropertyName("fileName")]
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

public record FileUpdateResponse(
    string FileName,
    string FileContent,
    string FileCreationDate,
    string LastModifiedDate,
    int FileSizeInBytes
) : FileReadResponse(FileName, FileContent, FileCreationDate, LastModifiedDate, FileSizeInBytes);

public record FileDeleteResponse(
    [property: JsonPropertyName("isDeleted")] bool IsDeleted
): BaseResponse(200);


public record FileShareResponse(
    [property: JsonPropertyName("fileShareUrl")]
    string FileShareLink
): BaseResponse(200);


public record FileShareReadResponse(
    [property: JsonPropertyName("fileName")]
    string FileName,
    [property: JsonPropertyName("fileContent")]
    string FileContent,
    [property: JsonPropertyName("fileSizeInBytes")]
    int FileSizeInBytes
): BaseResponse(200);

