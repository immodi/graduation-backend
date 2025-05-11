using backend.DTOs.Requests;
using backend.Models.HelperModels;
using backend.Services;

namespace backend.Interfaces;

public interface IUserRepository
{
    Task<DatabaseOutput> SignUpAsync(JwtService jwtService, AuthRequest request);
    Task<DatabaseOutput> SignInAsync(JwtService jwtService, AuthRequest request);
    Task<DatabaseOutput> GetUserRecoveryDataAsync(AuthResetRequest request);
    Task<DatabaseOutput> ResetPasswordAsync(JwtService jwtService, ResetRequest request);
    Task<DatabaseOutput> UpdateUserData(JwtService jwtService, string userToken, AuthUpdateRequest request);
    Task<DatabaseOutput> GetUserEmailAndUsername(JwtService jwtService, string userToken);
    
}
