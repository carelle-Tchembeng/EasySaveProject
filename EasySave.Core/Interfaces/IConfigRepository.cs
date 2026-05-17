// EasySave.Core/Interfaces/IConfigRepository.cs
// UNCHANGED from v1.1

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>Persistence contract for backup job list. Unchanged from v1.1.</summary>
    public interface IConfigRepository
    {
        List<BackupJob> Load();
        void Save(List<BackupJob> jobs);
    }
}
