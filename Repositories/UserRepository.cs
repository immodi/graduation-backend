using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Models.HelperModels;
using backend.Services;
using SQLite;

namespace backend.Repositories;


public class UserRepository(SQLiteAsyncConnection database) : IUserRepository
{
    public async Task<DatabaseOutput> SignUpAsync(JwtService jwtService, AuthRequest request)
    {
        var existingUser = await database.Table<User>()
            .Where(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (existingUser != null)
            return new DatabaseOutput(false, new ErrorResponse("Username already exists"));

        
        var existingUserWithEmail = await database.Table<User>()
            .Where(u => u.Email == request.Email)
            .FirstOrDefaultAsync();

        if (existingUserWithEmail != null)
            return new DatabaseOutput(false, new ErrorResponse("Username with that email already exists"));

        
        var passwordHash = PasswordHasher.HashPassword(request.Password);
        var resetCode = RandomStringGenerator.GenerateRandomString();

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            ResetCode = resetCode
        };

        await database.InsertAsync(newUser);
        return await SignInAsync(jwtService, request);
    }

    public async Task<DatabaseOutput> SignInAsync(JwtService jwtService, AuthRequest request)
    {
        var user = await database.Table<User>()
            .Where(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (user == null)
            return new DatabaseOutput(false, new ErrorResponse("Username not found"){StatusCode = 401});

        var passwordHash = PasswordHasher.HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
            return new DatabaseOutput(false, new ErrorResponse("Invalid password"){StatusCode = 401});

        var token = jwtService.GenerateToken(user.Username);
        return new DatabaseOutput(true, new AuthResponse(user.Id, token));
    }

    public async Task<DatabaseOutput> GetUserRecoveryDataAsync(AuthResetRequest request)
    {
        try
        {
            var user = await database.Table<User>()
                .Where(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null)
                return new DatabaseOutput(false, new ErrorResponse("User not found"));

            return new DatabaseOutput(true, new ResetData(user.Email, user.ResetCode));
        }
        catch (Exception ex)
        {
            return new DatabaseOutput(false, new ErrorResponse($"Password reset failed: {ex.Message}"));
        }
    }

    public async Task<DatabaseOutput> ResetPasswordAsync(JwtService jwtService, ResetRequest request)
    {
        try
        {
            var user = await database.Table<User>()
                .Where(u => u.Username == request.Username && u.ResetCode == request.Code)
                .FirstOrDefaultAsync();

            if (user == null)
                return new DatabaseOutput(false, new ErrorResponse("User not found or the reset code is incorrect"){StatusCode = 401});

            if (user.ResetCode != request.Code)
                return new DatabaseOutput(false, new ErrorResponse("This reset code is expired, please request a new one"){StatusCode = 401});

            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.ResetCode = RandomStringGenerator.GenerateRandomString();

            await database.UpdateAsync(user);

            return await SignInAsync(jwtService, new AuthRequest(user.Username, user.Email, request.NewPassword));
        }
        catch (Exception ex)
        {
            return new DatabaseOutput(false, new ErrorResponse($"Password reset failed: {ex.Message}"){StatusCode = 500});
        }
    }

    public async Task<DatabaseOutput> UpdateUserData(JwtService jwtService, string userToken, AuthUpdateRequest request)
    {
        try
        {
            var tokenOwner = jwtService.GetUsernameFromToken(userToken);
            // Retrieve the user by username
            var user = await database.Table<User>()
                .Where(u => u.Username == tokenOwner)
                .FirstOrDefaultAsync();
              
            if (user == null)
            {
                // User not found
                return new DatabaseOutput(false, new ErrorResponse("User not found"));
            }

            if (request.Username != null)
            {
                user.Username = request.Username;
            }

            if (request.Email != null)
            {
                user.Email = request.Email;
            }
            
            await database.UpdateAsync(user);
            return new DatabaseOutput(true, new AuthUpdateResponse(user.Username, user.Email));
        }
        catch (Exception ex)
        {
            return new DatabaseOutput(false, new ErrorResponse($"Password reset failed: {ex.Message}"){StatusCode = 500});
        }
    }
}
