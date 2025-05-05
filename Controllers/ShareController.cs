using System.Text.Json;
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
            return new ErrorResponse("An error occured, please try again later") { StatusCode = 500 };
        }
    }

    public async Task<BaseResponse> ReadSharedFile()
    {
        var queryParams = httpContext.Request.Query;

        try
        {
            // Step 1: Attempt to read the request body to get FileShareReadRequest
            FileShareReadRequest? request = null;
            if (httpContext.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(httpContext.Request.Body);
                var bodyText = await reader.ReadToEndAsync();

                if (!string.IsNullOrWhiteSpace(bodyText))
                {
                    // Deserialize the body into FileShareReadRequest
                    request = JsonSerializer.Deserialize<FileShareReadRequest>(bodyText);
                }
            }

            // Step 2: If no valid request in the body, try to get 'fileShareCode' from the query parameters
            var finalShareCode = "";

            if (request != null && !string.IsNullOrEmpty(request.FileShareCode))
            {
                finalShareCode = request.FileShareCode; // Use fileShareCode from body request
            }
            else
            {
                if (!queryParams.TryGetValue("fileShareCode", out var fileShareCodeValue) ||
                    string.IsNullOrEmpty(fileShareCodeValue))
                {
                    return new ErrorResponse("Invalid share code");
                }

                finalShareCode = fileShareCodeValue;
            }

            // Step 3: Use the finalShareCode to fetch the shared file
            var databaseOutput = await databaseService.ReadSharedFile(finalShareCode);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occurred, please try again later") { StatusCode = 500 };
        }
    }

    public async Task<BaseResponse> UpdateSharedFile(SharedFileUpdateRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }

        try
        {
            if (string.IsNullOrEmpty(request.FileShareCode))
            {
                return new ErrorResponse("Invalid file share code");
            }

            var databaseOutput = await databaseService.UpdateSharedFile(request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later") { StatusCode = 500 };
        }
    }
}
