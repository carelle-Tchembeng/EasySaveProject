// EasySave.Core/Interfaces/IStateRepository.cs
// UNCHANGED from v1.1

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>Real-time state writing contract. Unchanged from v1.1.</summary>
    public interface IStateRepository
    {
        void Update(List<BackupJob> jobs);
        void Clear(List<BackupJob> jobs);
    }
}
