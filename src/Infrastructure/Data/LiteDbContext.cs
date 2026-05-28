using LiteDB;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data;

/// <summary>
/// Wrapper around LiteDatabase.
/// Registered as Singleton because LiteDB works best with a single long-lived instance
/// per application (avoids multiple file locks and connection overhead).
/// We intentionally do not implement IDisposable here — disposing a singleton
/// would be incorrect, and the underlying resources are released when the process exits.
/// </summary>
public class LiteDbContext
{
    private readonly LiteDatabase _db;

    public ILiteCollection<User> Users { get; }
    public ILiteCollection<TaskItem> Tasks { get; }

    public LiteDbContext(string connectionString = "TaskManagement.db")
    {
        _db = new LiteDatabase(connectionString);

        Users = _db.GetCollection<User>("users");
        Tasks = _db.GetCollection<TaskItem>("tasks");

        // Indexes
        Users.EnsureIndex(x => x.Email, unique: true);
        Tasks.EnsureIndex(x => x.UserId);
    }
}
