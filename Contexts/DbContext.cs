using backend.Models;
using SQLite;

namespace backend.Contexts;

public class DbContext
{
    public readonly SQLiteAsyncConnection Database;

    public DbContext(string dbPath)
    {
        Database = new SQLiteAsyncConnection(dbPath);
        Task.Run(InitializeDatabase).Wait(); 
    }

    private async Task InitializeDatabase()
    {
        await Database.CreateTableAsync<User>();
        await Database.CreateTableAsync<Models.File>();
    }
}
