// EasySave.Core/Interfaces/ILogger.cs
// UNCHANGED from v1.1

using EasySave.Core.ValueObjects;

namespace EasySave.Core.Interfaces
{
    /// <summary>Log writing contract. Unchanged from v1.1.</summary>
    public interface ILogger
    {
        void Log(LogEntry entry);
    }
}
