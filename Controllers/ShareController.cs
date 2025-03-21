using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class ShareController(HttpContext httpContext, JwtService jwtService, DatabaseService databaseService)
{
    private readonly string _originUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";  
    
    public async Task<BaseResponse> ShareFile(string userToken, FileShareRequest? request)
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

            var databaseOutput = await databaseService.ShareFile(_originUrl, userToken, jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }

    public async Task<BaseResponse> ReadSharedFile(FileShareReadRequest? request)
    {
        
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (string.IsNullOrEmpty(request.FileShareCode))
            {
                return new ErrorResponse("Invalid share code");
            }
            
            var databaseOutput = await databaseService.ReadSharedFile(request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
}