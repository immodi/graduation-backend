using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

internal class LoginController(JwtService jwtService, DatabaseService databaseService, AuthRequest authRequest)
{
    public async Task<BaseResponse> LoginUser()
    {
        if (string.IsNullOrEmpty(authRequest.Username))
        {
            return new ErrorResponse("Username is required");
        }

        if (string.IsNullOrEmpty(authRequest.Password))
        {
            return new ErrorResponse("Password is required");
        }
        
        var isLoggedIn = await databaseService.SignInAsync(authRequest);
        if (!isLoggedIn)
        {
            return new ErrorResponse("Either Username or Password are wrong");
        }
        
        var token = jwtService.GenerateToken(authRequest.Username);
        return new AuthResponse(token);
    }
}