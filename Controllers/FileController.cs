using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class FileController(DatabaseService databaseService, BaseRequest? request)
{
    public async Task<BaseResponse> CreateFile()
    {
        if (request == null)
        {
            return new ErrorResponse("Invalid request");
        }
        var creationRequest = (request as FileCreationRequest)!;

        if (string.IsNullOrEmpty(creationRequest.FileContent))
        {
            return new ErrorResponse("Invalid file content");
        }
        
        if (string.IsNullOrEmpty(creationRequest.FileName))
        {
            return new ErrorResponse("Invalid file name");
        }

        var databaseOutput = await databaseService.CreateFile(creationRequest);
        return databaseOutput.Response;
    }
    
    public async Task<BaseResponse> UpdateFile()
    {
        if (request == null)
        {
            return new ErrorResponse("Invalid request");
        }
        var updateRequest = (request as FileUpdateRequest)!;

        if (updateRequest.FileId < 1)
        {
            return new ErrorResponse("Invalid file ID");
        }

        var databaseOutput = await databaseService.UpdateFile(updateRequest);
        return databaseOutput.Response;
    }
    
    public async Task<BaseResponse> ReadFile()
    {
        if (request == null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        var readRequest = (request as FileReadRequest)!;

        if (readRequest.FileId < 1)
        {
            return new ErrorResponse("Invalid file ID");
        }
        
        var databaseOutput = await databaseService.ReadFile(readRequest);
        return databaseOutput.Response;
    }
}
