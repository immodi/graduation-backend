using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

internal class AuthController(JwtService jwtService, DatabaseService databaseService)
{
    public async Task<BaseResponse> LoginUser(AuthRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (string.IsNullOrEmpty(request.Username))
            {
                return new ErrorResponse("Username is required");
            }

            if (!Regex.IsMatch(request.Username, @"^\w+$"))
            {
                return new ErrorResponse(
                    "Username can only contain letters, digits, and underscores, and must not have spaces");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return new ErrorResponse("Password is required");
            }

            var databaseOutput = await databaseService.SignInAsync(jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
        
    }
    
    public async Task<BaseResponse> RegisterUser(AuthRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        try
        {
            if (string.IsNullOrEmpty(request.Username))
            {
                return new ErrorResponse("Username is required");
            }

            if (!Regex.IsMatch(request.Username, @"^\w+$"))
            {
                return new ErrorResponse(
                    "Username can only contain letters, digits, and underscores, and must not have spaces");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return new ErrorResponse("Password is required");
            }

            var databaseOutput = await databaseService.SignUpAsync(jwtService, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
    }
}