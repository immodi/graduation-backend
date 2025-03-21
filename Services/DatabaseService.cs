using System.Security.Cryptography;
using System.Text;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Helpers;
using backend.Models;
using backend.Models.HelperModels;
using File = backend.Models.File;

namespace backend.Services;

using SQLite;
using System.Threading.Tasks;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        InitializeDatabase().Wait();
    }

    private async Task InitializeDatabase()
    {
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<File>();
    }

    public async Task<DatabaseOutput> SignUpAsync(JwtService jwtService, AuthRequest request)
    {
        // Check if the username already exists
        var existingUser = await _database.Table<User>()
            .Where(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return new DatabaseOutput(false, new ErrorResponse("Username already exists"));
        }

        // Hash the password (implement a proper hashing mechanism)
        var passwordHash = PasswordHasher.HashPassword(request.Password);

        // Create a new user
        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash
        };

        // Insert the new user into the database
        await _database.InsertAsync(newUser);
        return await SignInAsync(jwtService, request);
    }

    public async Task<DatabaseOutput> SignInAsync(JwtService jwtService, AuthRequest request)
    {
        // Retrieve the user by username
        var user = await _database.Table<User>()
            .Where(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            // User not found
            return new DatabaseOutput(false, new ErrorResponse("Username not found"));
        }

        // Hash the input password and compare with the stored hash
        var passwordHash = PasswordHasher.HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
        {
            return new DatabaseOutput(false, new ErrorResponse("Invalid password"));
        }
        
        var token = jwtService.GenerateToken(user.Username);
        return new DatabaseOutput(true, new AuthResponse(user.Id, token));
    }
    
    public async Task<DatabaseOutput> ReadFile(string userToken, JwtService jwtService, FileReadRequest request)
    {
        // Retrieve the user by username
        var file = await _database.Table<File>()
            .Where(f => f.Id == request.FileId)
            .FirstOrDefaultAsync();
        
        if (file == null)
        {
            // File not found
            return new DatabaseOutput(false, new ErrorResponse("File not found"));
        }
        
        var user = await _database.Table<User>()
            .Where(u => u.Id == file.UserId)
            .FirstOrDefaultAsync();
        
        
        var isTokenValidForUser = jwtService.IsTokenValidForUser(userToken, user.Username);

        if (!isTokenValidForUser)
        {
            return new DatabaseOutput(false, new ErrorResponse("The resource isn't yours"){StatusCode = 403});
        }
        
        return new DatabaseOutput(true, new FileReadResponse(file.FileName, file.Content,  file.CreationDate, file.LastModifiedDate, Encoding.UTF8.GetBytes(file.Content).Length));
    }

    public async Task<DatabaseOutput> GetAllUserFiles(string userToken, JwtService jwtService)
    {
              var tokenOwner = jwtService.GetUsernameFromToken(userToken);
              // Retrieve the user by username
              var user = await _database.Table<User>()
                  .Where(u => u.Username == tokenOwner)
                  .FirstOrDefaultAsync();
              
              if (user == null)
              {
                  // User not found
                  return new DatabaseOutput(false, new ErrorResponse("User not found"));
              }

              var files = await _database.Table<File>()
                  .Where(f => f.UserId == user.Id).ToArrayAsync();

              return new DatabaseOutput(true, new AllFilesResponse(files.Select(file => new SingleFileResponse(file.Id, file.FileName, Encoding.UTF8.GetBytes(file.Content).Length)).ToArray()));
    }
    
    public async Task<DatabaseOutput> CreateFile(string userToken, JwtService jwtService, FileCreationRequest request)
    {
        var tokenOwner = jwtService.GetUsernameFromToken(userToken);
        
        // Retrieve the user by username
        var user = await _database.Table<User>()
            .Where(u => u.Username == tokenOwner)
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
            // User not found
            return new DatabaseOutput(false, new ErrorResponse("User not found"));
        }
      
        var newFile = new File
        {
            FileName = request.FileName,
            UserId = user.Id,
            Content = request.FileContent,
        };

        await _database.InsertAsync(newFile);
        return new DatabaseOutput(true, new FileCreationResponse(newFile.Id, newFile.FileName, Encoding.UTF8.GetBytes(newFile.Content).Length));
    }
    
    public async Task<DatabaseOutput> UpdateFile(string userToken, JwtService jwtService, FileUpdateRequest request)
    {
        // Retrieve the file by its ID
        var file = await _database.Table<File>()
            .Where(f => f.Id == request.FileId)
            .FirstOrDefaultAsync();

        if (file == null)
        {
            return new DatabaseOutput(false, new ErrorResponse("File not found"));
        }

        if (!string.IsNullOrWhiteSpace(request.NewFileName))
        {
            file.FileName = request.NewFileName;
        }

        if (!string.IsNullOrWhiteSpace(request.NewFileContent))
        {
            file.Content = request.NewFileContent;
        }
        
        var user = await _database.Table<User>()
            .Where(u => u.Id == file.UserId)
            .FirstOrDefaultAsync();
        
        
        var isTokenValidForUser = jwtService.IsTokenValidForUser(userToken, user.Username);

        if (!isTokenValidForUser)
        {
            return new DatabaseOutput(false, new ErrorResponse("The resource isn't yours"){StatusCode = 403});
        }

        file.LastModifiedDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

        // Save changes to the database
        await _database.UpdateAsync(file);
        return new DatabaseOutput(true, new FileUpdateResponse(file.FileName, file.Content,  file.CreationDate, file.LastModifiedDate, Encoding.UTF8.GetBytes(file.Content).Length));
    }

    public async Task<DatabaseOutput> DeleteFile(string userToken, JwtService jwtService, FileDeleteRequest request)
    {
        // Retrieve the user by username
        var file = await _database.Table<File>()
            .Where(f => f.Id == request.FileId)
            .FirstOrDefaultAsync();
        
        if (file == null)
        {
            // File not found
            return new DatabaseOutput(false, new ErrorResponse("File not found"));
        }
        
        var user = await _database.Table<User>()
            .Where(u => u.Id == file.UserId)
            .FirstOrDefaultAsync();
        
        var isTokenValidForUser = jwtService.IsTokenValidForUser(userToken, user.Username);

        if (!isTokenValidForUser)
        {
            return new DatabaseOutput(false, new ErrorResponse("The resource isn't yours"){StatusCode = 403});
        }
        
        await _database.DeleteAsync(file);
        return new DatabaseOutput(true, new FileDeleteResponse(true));
    }

    
    public async Task<DatabaseOutput> ShareFile(string originUrl, string userToken, JwtService jwtService, FileShareRequest request)
    {
        // Retrieve the file by its ID
        var file = await _database.Table<File>()
            .Where(f => f.Id == request.FileId)
            .FirstOrDefaultAsync();
        
        if (file == null)
        {
            return new DatabaseOutput(false, new ErrorResponse("File not found"));
        }
        
        var user = await _database.Table<User>()
            .Where(u => u.Id == file.UserId)
            .FirstOrDefaultAsync();
        
        var isTokenValidForUser = jwtService.IsTokenValidForUser(userToken, user.Username);
        
        if (!isTokenValidForUser)
        {
            return new DatabaseOutput(false, new ErrorResponse("The resource isn't yours"){StatusCode = 403});
        }
        
        file.SharingCode = RandomStringGenerator.GenerateRandomString();
        await _database.UpdateAsync(file);
        
        return new DatabaseOutput(true, new FileShareResponse(file.SharingCode));
    }

    public async Task<DatabaseOutput> ReadSharedFile(FileShareReadRequest request)
    {
        var fileShareCode = request.FileShareCode;

        // Retrieve the file by its ID
        var file = await _database.Table<File>()
            .Where(f => f.SharingCode == fileShareCode)
            .FirstOrDefaultAsync();
        
        if (file == null)
        {
            return new DatabaseOutput(false, new ErrorResponse("File not found, or share code was previously overriden"));
        }
        
        return new DatabaseOutput(true, new FileShareReadResponse(file.FileName, file.Content, Encoding.UTF8.GetBytes(file.Content).Length));
    }
}
