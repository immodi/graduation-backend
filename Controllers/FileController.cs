using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class FileController(HttpContext httpContext, JwtService jwtService, DatabaseService databaseService)
{
    private readonly string _userToken = httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
       
    public async Task<BaseResponse> ReadFile(FileReadRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (request.FileId < 1)
            {
                return new ErrorResponse("Invalid file ID");
            }

            var databaseOutput = await databaseService.ReadFile(_userToken, jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
    public async Task<BaseResponse> GetAllFiles(AllFilesRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            var databaseOutput = await databaseService.GetAllUserFiles(_userToken, jwtService);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
    public async Task<BaseResponse> CreateFile(FileCreationRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (string.IsNullOrEmpty(request.FileContent))
            {
                return new ErrorResponse("Invalid file content");
            }
        
            if (string.IsNullOrEmpty(request.FileName))
            {
                return new ErrorResponse("Invalid file name");
            }

            var databaseOutput = await databaseService.CreateFile(_userToken, jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
    public async Task<BaseResponse> UpdateFile(FileUpdateRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (request.FileId < 1)
            {
                return new ErrorResponse("Invalid file ID");
            }

            var databaseOutput = await databaseService.UpdateFile(_userToken, jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
    public async Task<BaseResponse> DeleteFile(FileDeleteRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (request.FileId < 1)
            {
                return new ErrorResponse("Invalid file Id");
            }

            var databaseOutput = await databaseService.DeleteFile(_userToken, jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
    
    
}
