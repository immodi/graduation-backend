using System.Security.Cryptography;
using System.Text;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Models;

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
    }

    public async Task<DatabaseOutput> SignUpAsync(AuthRequest request)
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
        var passwordHash = HashPassword(request.Password);

        // Create a new user
        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash
        };

        // Insert the new user into the database
        await _database.InsertAsync(newUser);
        return new DatabaseOutput(true, new AuthResponse(newUser.Username));
    }

    public async Task<DatabaseOutput> SignInAsync(AuthRequest request)
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
        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
        {
            return new DatabaseOutput(false, new ErrorResponse("Invalid password"));
        }
        
        return new DatabaseOutput(true, new AuthResponse(user.Username));
    }
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
        
    }

    
}
