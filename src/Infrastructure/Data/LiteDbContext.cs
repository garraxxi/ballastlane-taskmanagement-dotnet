using LiteDB;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data;

public class LiteDbContext : IDisposable
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

    public void Dispose() => _db.Dispose();
}
