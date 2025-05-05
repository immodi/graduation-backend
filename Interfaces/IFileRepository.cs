using backend.DTOs.Requests;
using backend.Models.HelperModels;
using backend.Services;

namespace backend.Interfaces;

public interface IFileRepository
{
    Task<DatabaseOutput> ReadFile(string userToken, JwtService jwtService, int fileId);
    Task<DatabaseOutput> GetAllUserFiles(string userToken, JwtService jwtService);
    Task<DatabaseOutput> CreateFile(string userToken, JwtService jwtService, FileCreationRequest request);
    Task<DatabaseOutput> UpdateFile(string userToken, JwtService jwtService, FileUpdateRequest request);
    Task<DatabaseOutput> DeleteFile(string userToken, JwtService jwtService, FileDeleteRequest request);
    Task<DatabaseOutput> ShareFile(string originUrl, string userToken, JwtService jwtService, FileShareRequest request);
    Task<DatabaseOutput> ReadSharedFile(string fileShareCode);
}