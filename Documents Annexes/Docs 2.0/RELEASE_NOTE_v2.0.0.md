# Release Note — EasySave v2.0.0

**ProSoft** | Release date: November 2024

---

## Overview

EasySave v2.0.0 is a **major release** that introduces a graphical user interface, file encryption via CryptoSoft, business software detection, and unlimited backup jobs. It supersedes v1.1.0 for most clients, while v1.1.0 remains available for clients who cannot upgrade.

---

## What's New

### 1. Graphical Interface — WPF / MVVM

The console interface is replaced by a full **WPF graphical application** built on the **MVVM architecture pattern**.

Key interface features:
- Main window with DataGrid showing all backup jobs and real-time progress bars
- Job creation and editing via dialog windows
- Settings panel for encryption, business software, and log format configuration
- Status bar with execution feedback and busy indicator

The CLI mode (`EasySave.exe 1-3`, `EasySave.exe 1;3`) is fully preserved. When arguments are detected at startup, the application runs headlessly without opening the main window, maintaining full compatibility with existing scheduled tasks and scripts.

---

### 2. Unlimited Backup Jobs

The 5-job limit introduced in v1.0 is removed. Backup jobs are now identified by a `Guid` instead of a sequential integer, enabling an unlimited number of jobs without index conflicts.

> **Breaking change:** The `config.json` schema is not compatible with v1.x. Job IDs changed from `int` to `Guid`. Existing jobs must be recreated after upgrading.

---

### 3. CryptoSoft File Encryption

EasySave can now encrypt files using the external **CryptoSoft** tool after copying them to the target directory.

**Configuration (Settings dialog):**
- Full path to `CryptoSoft.exe`
- List of file extensions to encrypt (e.g. `.pdf`, `.docx`, `.xlsx`)

**Behaviour:**
- Files with matching extensions are encrypted after each copy
- Files with other extensions are copied without encryption
- If CryptoSoft is unavailable, the file is copied but not encrypted (`EncryptionTimeMs = -1` in log)

**New log fields:**

| Field | Values |
|---|---|
| `encryptionTimeMs` | `0` = not encrypted, `> 0` = duration ms, `< 0` = error |
| `isEncrypted` | `true` if successfully encrypted |

---

### 4. Business Software Detection

EasySave monitors a configurable process name before and during each backup execution.

**Before launch:** if the business software is detected, the job is immediately blocked. The event is logged and an error is displayed.

**During execution:** if detected while a file is being copied, EasySave finishes the current file transfer (as required by the specification), then stops the backup. The interruption is logged.

**Configuration:** the process name is set in Settings (e.g. `calc` for Windows Calculator during demonstrations).

---

### 5. Log Format (inherited from v1.1)

JSON and XML log formats remain available, now also selectable from the graphical Settings panel. The EasyLog DLL v2.0.0 adds the two new fields `encryptionTimeMs` and `isEncrypted` to both formats.

---

## Technical Specifications

| Property | Value |
|---|---|
| Target framework | .NET 8.0 Windows |
| UI framework | WPF (Windows Presentation Foundation) |
| Architecture pattern | MVVM |
| Language | C# |
| New files | 16 (EasySave.WPF layer entirely new) |
| Modified files | 12 (Core, Infrastructure, EasyLog) |
| Core layer changes | AppConfiguration (new), LogEntry (+2 fields), BackupJob (Guid), BackupService (+2 deps) |

---

## Files Added

| Layer | File | Description |
|---|---|---|
| Core | `Entities/AppConfiguration.cs` | User-configurable settings entity |
| Core | `Interfaces/IEncryptionService.cs` | CryptoSoft abstraction |
| Core | `Interfaces/IBusinessSoftwareDetector.cs` | Process detection abstraction |
| Core | `Interfaces/IAppConfigRepository.cs` | AppConfiguration persistence contract |
| Infrastructure | `Encryption/CryptoSoftAdapter.cs` | Launches CryptoSoft.exe via Process.Start() |
| Infrastructure | `Detection/BusinessSoftwareDetector.cs` | Process.GetProcessesByName() implementation |
| Infrastructure | `Repositories/JsonAppConfigRepository.cs` | appconfig.json persistence |
| EasyLog | `DTOs/LogEntryDto.cs` (updated) | + EncryptionTimeMs, IsEncrypted |
| WPF | `App.xaml / App.xaml.cs` | Startup, DI wiring, CLI detection |
| WPF | `Commands/RelayCommand.cs` | ICommand implementation |
| WPF | `ViewModels/ViewModelBase.cs` | INotifyPropertyChanged base |
| WPF | `ViewModels/MainViewModel.cs` | Main window ViewModel |
| WPF | `ViewModels/BackupJobViewModel.cs` | Per-job ViewModel |
| WPF | `ViewModels/JobEditorViewModel.cs` | Create/Edit form ViewModel |
| WPF | `ViewModels/SettingsViewModel.cs` | Settings ViewModel |
| WPF | `Views/MainWindow.xaml` | Main window XAML |
| WPF | `Views/JobEditorWindow.xaml` | Job editor dialog XAML |
| WPF | `Views/SettingsWindow.xaml` | Settings dialog XAML |
| WPF | `Localization/LocalizationService.cs` | FR/EN service with LanguageChanged event |
| WPF | `DI/ServiceContainer.cs` | Lightweight DI container |

---

## Files Modified

| Layer | File | Change |
|---|---|---|
| Core | `Entities/BackupJob.cs` | Id: int → Guid |
| Core | `ValueObjects/LogEntry.cs` | + EncryptionTimeMs, IsEncrypted, SuccessWithEncryption() |
| Core | `Interfaces/IBackupStrategy.cs` | Execute() + IEncryptionService + AppConfiguration |
| Core | `Services/JobManager.cs` | No MaxJobs limit, Guid-based operations |
| Core | `Services/BackupService.cs` | + IEncryptionService, + IBusinessSoftwareDetector, + AppConfiguration |
| EasyLog | `Formatters/XmlLogFormatter.cs` | Serialize EncryptionTimeMs and IsEncrypted |
| Infrastructure | `Strategies/FullBackupStrategy.cs` | + ShouldEncrypt(), 5-param callback |
| Infrastructure | `Strategies/DifferentialBackupStrategy.cs` | Same |
| Infrastructure | `Helpers/PathHelper.cs` | + GetAppConfigPath() |
| Infrastructure | `Configuration/AppSettings.cs` | + AppConfigFilePath |
| Infrastructure | `Logging/EasyLogAdapter.cs` | MapToDto() handles EncryptionTimeMs |

---

## Breaking Changes

| Change | Impact | Migration |
|---|---|---|
| `config.json` schema: int IDs → Guid IDs | Existing job configs not loadable | Recreate jobs manually after upgrade |
| `EasySave.ConsoleApp` removed | Console-specific tooling no longer available | Use CLI mode via `EasySave.exe` arguments |
| `IBackupStrategy.Execute()` signature changed | Custom strategies (if any) need updating | Add `IEncryptionService` and `AppConfiguration` parameters |

---

## Known Limitations

| Limitation | Detail |
|---|---|
| Windows only | WPF is Windows-specific — Linux/macOS not supported |
| CryptoSoft required | File encryption requires a separate CryptoSoft installation |
| No parallel execution | Jobs still execute sequentially (planned for v3.0) |
| No Play/Pause/Stop | Per-job controls deferred to v3.0 |

---

## Bug Fixes

No bug fixes — this is a major feature release.

---

## Upgrade Notes

### From v1.1.0

1. Replace `EasySave.exe` and `EasyLog.dll` with v2.0.0 binaries
2. Install **.NET 8.0 Windows Desktop Runtime** if not already present
3. **Do not copy existing `config.json`** — schema is incompatible
4. Recreate backup jobs via the graphical interface
5. Configure CryptoSoft and business software settings as needed
6. Existing log files and `state.json` remain fully usable

### From v1.0.0

Same steps as from v1.1.0. Additionally, `appsettings.json` did not exist in v1.0 — it will be created automatically.

---

## Compatibility

| Component | v1.x compatibility |
|---|---|
| `config.json` | **Not compatible** — Guid IDs vs int IDs |
| `state.json` | Compatible |
| Log files (.json / .xml) | Compatible — new fields simply absent in old files |
| `EasyLog.dll` v1.x | Not compatible with v2.0 executable — must be replaced |
| CLI arguments | Fully compatible — same syntax as v1.0 |

---

## Roadmap — Planned for v3.0

- Per-job Play / Pause / Stop controls in the WPF interface
- Parallel backup execution (multiple jobs simultaneously)
- Additional language support

---

*EasySave v2.0.0 — ProSoft © 2024. All rights reserved.*  
*Maintenance contract: 5/7, 08:00–17:00 — Annual renewal with SYNTEC index revaluation.*
