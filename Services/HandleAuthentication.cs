using System.Text;
using backend.DTOs.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public static class HandleAuthenticationClass
{
    public static AuthenticationBuilder HandleAuthentication(this IServiceCollection services, string jwtIssuer, string jwtKey)
    {
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
                
                Console.WriteLine(jwtIssuer);
                Console.WriteLine(jwtKey);

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse(); 

                        var response = Results.Json(
                            new ErrorResponse("Unauthorized access") { StatusCode = 401 },
                            statusCode: 401
                        );

                        await response.ExecuteAsync(context.HttpContext);
                    }
                };
            });
    }
}