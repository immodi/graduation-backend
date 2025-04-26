using System.Text.Json;
using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class FileController(HttpContext httpContext, JwtService jwtService, DatabaseService databaseService)
{
    private readonly string _userToken = httpContext.Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
       
    public async Task<BaseResponse> ReadFile()
    {
        var queryParams = httpContext.Request.Query;
        Console.WriteLine(queryParams.ToString());

        try
        {
            // Step 1: Attempt to read the request body to get FileReadRequest
            FileReadRequest? request = null;
            if (httpContext.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(httpContext.Request.Body);
                var bodyText = await reader.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(bodyText))
                {
                    // Deserialize the body into FileReadRequest
                    request = JsonSerializer.Deserialize<FileReadRequest>(bodyText);
                }
            }

            // Step 2: If no valid request, try to get 'fileId' from the query parameters
            var finalFileId = -1;

            if (request != null && request.FileId > 0)
            {
                finalFileId = request.FileId; // Use fileId from body request
            }
            else
            {
                if (!queryParams.TryGetValue("fileId", out var fileIdValue) || 
                    !int.TryParse(fileIdValue, out finalFileId) || 
                    finalFileId < 1)
                {
                    return new ErrorResponse("Invalid file ID");
                }
            }

            // Step 3: Use the finalFileId to fetch the file
            var databaseOutput = await databaseService.ReadFile(_userToken, jwtService, finalFileId);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occurred, please try again later") { StatusCode = 500 };
        }
    }

    public async Task<BaseResponse> GetAllFiles()
    {
        // if (request is null)
        // {
        //     return new ErrorResponse("Invalid request");
        // }
        
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
