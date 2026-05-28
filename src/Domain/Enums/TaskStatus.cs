using System.Text.Json.Serialization;

namespace TaskManagement.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
