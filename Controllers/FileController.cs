using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class FileController(HttpContext httpContext, JwtService jwtService, DatabaseService databaseService, BaseRequest? request)
{
    private readonly string _userToken = httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
       
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

        var databaseOutput = await databaseService.CreateFile(_userToken, jwtService, creationRequest);
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

        var databaseOutput = await databaseService.UpdateFile(_userToken, jwtService, updateRequest);
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
        
        var databaseOutput = await databaseService.ReadFile(_userToken, jwtService, readRequest);
        return databaseOutput.Response;
    }
}
