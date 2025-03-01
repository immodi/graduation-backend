using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class RegisterController(JwtService jwtService, DatabaseService databaseService, AuthRequest authRequest)
{
    public async Task<BaseResponse> RegisterUser()
    {
        if (string.IsNullOrEmpty(authRequest.Username))
        {
            return new ErrorResponse("Username is required");
        }

        if (string.IsNullOrEmpty(authRequest.Password))
        {
            return new ErrorResponse("Password is required");
        }


        var isRegistered = await databaseService.SignUpAsync(authRequest);
        if (!isRegistered)
        {
            return new ErrorResponse("Couldn't register user"){ StatusCode = 500 };
        }
        
        var token = jwtService.GenerateToken(authRequest.Username);
        return new AuthResponse(token);
    }
}