using System.Security.Cryptography;
using System.Text;
using backend.Contexts;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Helpers;
using backend.Interfaces;
using backend.Models;
using backend.Models.HelperModels;
using File = backend.Models.File;

namespace backend.Services;

using SQLite;
using System.Threading.Tasks;

public class DatabaseService(IUserRepository userRepository, IFileRepository fileRepository)
{
    // User methods
    public Task<DatabaseOutput> SignUpAsync(JwtService jwtService, AuthRequest request)
    {
        return userRepository.SignUpAsync(jwtService, request);
    }

    public Task<DatabaseOutput> SignInAsync(JwtService jwtService, AuthRequest request)
    {
        return userRepository.SignInAsync(jwtService, request);
    }

    public Task<DatabaseOutput> GetUserRecoveryDataAsync(AuthResetRequest request)
    {
        return userRepository.GetUserRecoveryDataAsync(request);
    }

    public Task<DatabaseOutput> ResetPasswordAsync(JwtService jwtService, ResetRequest request)
    {
        return userRepository.ResetPasswordAsync(jwtService, request);
    }
    
    public Task<DatabaseOutput> UpdateUserDataAsync(JwtService jwtService, string userToken, AuthUpdateRequest request)
    {
        return userRepository.UpdateUserData(jwtService, userToken, request);
    }
    
    // File methods
    public Task<DatabaseOutput> ReadFile(string token, JwtService jwt, int fileId) =>
        fileRepository.ReadFile(token, jwt, fileId);

    public Task<DatabaseOutput> GetAllUserFiles(string token, JwtService jwt) =>
        fileRepository.GetAllUserFiles(token, jwt);

    public Task<DatabaseOutput> CreateFile(string token, JwtService jwt, FileCreationRequest req) =>
        fileRepository.CreateFile(token, jwt, req);

    public Task<DatabaseOutput> UpdateFile(string token, JwtService jwt, FileUpdateRequest req) =>
        fileRepository.UpdateFile(token, jwt, req);

    public Task<DatabaseOutput> DeleteFile(string token, JwtService jwt, FileDeleteRequest req) =>
        fileRepository.DeleteFile(token, jwt, req);

    public Task<DatabaseOutput> ShareFile(string origin, string token, JwtService jwt, FileShareRequest req) =>
        fileRepository.ShareFile(origin, token, jwt, req);

    public Task<DatabaseOutput> ReadSharedFile(string shareCode) =>
        fileRepository.ReadSharedFile(shareCode);
  
    public Task<DatabaseOutput> UpdateSharedFile(SharedFileUpdateRequest request) =>
        fileRepository.UpdateFileWithShareCode(request);

    
}
