namespace backend.Models;

using SQLite;

public class File
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Content { get; set; }

    [NotNull] 
    public string CreationDate { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");
    
    [NotNull] 
    public string LastModifiedDate { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"); 
    
    [NotNull]
    public int UserId { get; set; }
    
    public string FileName { get; set; }
    
}