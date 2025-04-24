using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;
using Microsoft.IdentityModel.Tokens;

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

    public async Task<BaseResponse> RequestResetPassword(string userToken)
    {
        if (string.IsNullOrEmpty(userToken))
        {   
            return new ErrorResponse("Invalid request");
        }

        try
        {
            var databaseOutput = await databaseService.GetUserRecoveryDataAsync(jwtService, userToken);
            var userRecoveryData = databaseOutput.Response as ResetData;
            var resetResponse = await EmailService.Send(userRecoveryData.Email, userRecoveryData.Code);
            return resetResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
        
    }
    
    public async Task<BaseResponse> ResetPassword(string userToken, ResetRequest? request)
    {
        if (request is null)
        {
            return new ErrorResponse("Invalid request");
        }

        try
        {
            var databaseOutput = await databaseService.ResetPasswordAsync(jwtService, userToken, request);
            return databaseOutput.Response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
        }
        
    }


}