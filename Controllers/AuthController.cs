using System.Text.RegularExpressions;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

internal class AuthController(JwtService jwtService, DatabaseService databaseService, AuthRequest? authRequest)
{
    public async Task<BaseResponse> LoginUser()
    {
        if (authRequest == null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        if (string.IsNullOrEmpty(authRequest.Username))
        {
            return new ErrorResponse("Username is required");
        }
        
        if (!Regex.IsMatch(authRequest.Username, @"^\w+$"))
        {
            return new ErrorResponse("Username can only contain letters, digits, and underscores, and must not have spaces");
        }
        
        if (string.IsNullOrEmpty(authRequest.Password))
        {
            return new ErrorResponse("Password is required");
        }
        
        var databaseOutput = await databaseService.SignInAsync(jwtService, authRequest);
        return databaseOutput.Response;
        
    }
    
    public async Task<BaseResponse> RegisterUser()
    {
        if (authRequest == null)
        {
            return new ErrorResponse("Invalid request");
        }
        
        if (string.IsNullOrEmpty(authRequest.Username))
        {
            return new ErrorResponse("Username is required");
        }
        
        if (!Regex.IsMatch(authRequest.Username, @"^\w+$"))
        {
            return new ErrorResponse("Username can only contain letters, digits, and underscores, and must not have spaces");
        }
        
        if (string.IsNullOrEmpty(authRequest.Password))
        {
            return new ErrorResponse("Password is required");
        }

        var databaseOutput = await databaseService.SignUpAsync(jwtService, authRequest);
        return databaseOutput.Response;
    }
}