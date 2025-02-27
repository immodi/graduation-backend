using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Services;

namespace backend.Controllers;

public class RegisterController(JwtService jwtService, RegisterRequest registerRequest)
{
    public BaseResponse RegisterUser()
    {
        if (string.IsNullOrEmpty(registerRequest.Username))
        {
            return new ErrorResponse("Username is required");
        }

        if (string.IsNullOrEmpty(registerRequest.Password))
        {
            return new ErrorResponse("Password is required");
        }
        var token = jwtService.GenerateToken(registerRequest.Username);
        return new RegisterResponse(token);
    }
}