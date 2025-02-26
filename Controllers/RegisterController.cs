using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public class RegisterController(JwtService jwtService, [FromBody] string username)
{
    public object RegisterUser()
    {
        if (string.IsNullOrEmpty(username))
        {
            return Results.BadRequest("Username is required");
        }

        var token = jwtService.GenerateToken(username);
        return new { token };
    }
}