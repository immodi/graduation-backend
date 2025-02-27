using backend.DTOs.Responses;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class RegisterController(JwtService jwtService, [FromBody] string username)
{
    public BaseResponse RegisterUser()
    {
        if (string.IsNullOrEmpty(username))
        {
            return new ErrorResponse("Username is required");
        }

        var token = jwtService.GenerateToken(username);
        return new RegisterResponse(token);
    }
}