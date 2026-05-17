# EasySave v2.0

> Backup software developed by **ProSoft** — WPF graphical application built with .NET 8.0

---

## Table of Contents

- [Overview](#overview)
- [What's New in v2.0](#whats-new-in-v20)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
  - [Graphical Interface](#graphical-interface)
  - [Command-Line Mode](#command-line-mode)
  - [Settings](#settings)
- [Configuration](#configuration)
- [Log Files](#log-files)
- [Architecture](#architecture)
- [Versioning](#versioning)
- [Contributing](#contributing)

---

## Overview

EasySave v2.0 is a major release of the ProSoft backup solution. It replaces the console interface with a full **WPF graphical interface** built on the **MVVM pattern**, and introduces file encryption via **CryptoSoft**, business software detection, and unlimited backup jobs.

| | |
|---|---|
| **Version** | 2.0.0 |
| **Previous version** | 1.1.0 |
| **License** | Proprietary — ProSoft |
| **Platform** | Windows 10 / Windows Server 2016 or later |
| **Runtime** | .NET 8.0 Windows |
| **Language** | C# / WPF |

---

## What's New in v2.0

### Graphical Interface (WPF / MVVM)

The console application is replaced by a full graphical interface built with WPF and the MVVM architecture pattern. The main window displays all backup jobs in a DataGrid with real-time progress bars. Dialogs allow creating, editing, and configuring jobs without a command-line.

The CLI mode is fully preserved — `EasySave.exe 1-3` or `EasySave.exe 1;3` still work as in v1.0 and v1.1.

### Unlimited Backup Jobs

The 5-job limit has been removed. Jobs are now identified by a `Guid` instead of a sequential integer index, enabling an unlimited number of backup jobs without index conflicts.

### CryptoSoft File Encryption

EasySave can now encrypt files after copying them to the target directory using the external **CryptoSoft** tool. The user configures:

- The path to `CryptoSoft.exe` in Settings
- The list of file extensions to encrypt (e.g. `.pdf`, `.docx`, `.xlsx`)

Files whose extension is not in the list are copied without encryption. The encryption time is recorded in the log file.

| `EncryptionTimeMs` value | Meaning |
|---|---|
| `0` | File not encrypted (extension not in list) |
| `> 0` | Encryption duration in milliseconds |
| `< 0` | CryptoSoft error code |

### Business Software Detection

EasySave monitors a configurable process name before and during each backup execution.

- **Before launch**: if the business software is running, the job is blocked and logged.
- **During execution**: if detected mid-backup, the current file copy is completed first, then execution stops and the interruption is logged.

The business software name is configured in Settings. The Windows Calculator (`calc`) can be used as a substitute during demonstrations.

### Log File Format (inherited from v1.1)

JSON and XML log formats remain available, selectable in Settings.

---

## Prerequisites

| Requirement | Version |
|---|---|
| Operating System | Windows 10 (64-bit) / Windows Server 2016 or later |
| .NET Runtime | [.NET 8.0 Windows Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| RAM | 512 MB available |
| Disk space | 50 MB for application + log files |
| CryptoSoft | Required only if file encryption is used |
| Visual Studio (dev only) | 2022 or later |

> **Important:** The WPF application requires the **.NET 8.0 Windows Desktop Runtime**, not just the base runtime.

---

## Project Structure

```
EasySave/
├── EasySave.sln
├── EasyLog/                          # Logging DLL v2.0
│   ├── DTOs/LogEntryDto.cs           # + EncryptionTimeMs, IsEncrypted
│   ├── Formatters/XmlLogFormatter.cs # + new fields in XML output
│   └── ...                           # unchanged from v1.1
├── EasySave.Core/                    # Business logic — no external dependencies
│   ├── Entities/
│   │   ├── AppConfiguration.cs       # NEW: CryptoSoftPath, BusinessSoftwareName, EncryptedExtensions
│   │   └── BackupJob.cs              # Id: Guid (unlimited jobs)
│   ├── Interfaces/
│   │   ├── IEncryptionService.cs     # NEW
│   │   ├── IBusinessSoftwareDetector.cs # NEW
│   │   └── IAppConfigRepository.cs  # NEW
│   ├── Services/
│   │   ├── BackupService.cs          # + encryption + business software detection
│   │   └── JobManager.cs             # no MaxJobs limit
│   └── ValueObjects/LogEntry.cs      # + EncryptionTimeMs, IsEncrypted
├── EasySave.Infrastructure/
│   ├── Detection/BusinessSoftwareDetector.cs  # NEW: Process.GetProcessesByName()
│   ├── Encryption/CryptoSoftAdapter.cs        # NEW: launches CryptoSoft.exe
│   ├── Repositories/JsonAppConfigRepository.cs # NEW: appconfig.json
│   ├── Strategies/FullBackupStrategy.cs       # + encryption + 5-param callback
│   └── Strategies/DifferentialBackupStrategy.cs # same
└── EasySave.WPF/                     # NEW — replaces ConsoleApp
    ├── App.xaml / App.xaml.cs        # Startup, DI wiring, CLI detection
    ├── Commands/RelayCommand.cs      # ICommand implementation
    ├── ViewModels/                   # MVVM ViewModels
    │   ├── ViewModelBase.cs
    │   ├── MainViewModel.cs
    │   ├── BackupJobViewModel.cs
    │   ├── JobEditorViewModel.cs
    │   └── SettingsViewModel.cs
    ├── Views/                        # WPF XAML Windows
    │   ├── MainWindow.xaml
    │   ├── JobEditorWindow.xaml
    │   └── SettingsWindow.xaml
    └── Localization/LocalizationService.cs
```

---

## Installation

### Option 1 — Build from source

```bash
# Clone and build
git clone https://github.com/ProSoft/EasySave.git
cd EasySave

dotnet build EasySave.sln --configuration Release

# Run
cd EasySave.WPF/bin/Release/net8.0-windows
EasySave.exe
```

### Option 2 — Visual Studio 2022

1. Open `EasySave.sln`
2. Set `EasySave.WPF` as the **Startup Project**
3. Press `F5`

### Upgrading from v1.1

1. Replace `EasySave.exe` and `EasyLog.dll` with v2.0 versions
2. Existing `config.json` is **not compatible** — job IDs changed from `int` to `Guid`. Export your job configurations manually before upgrading.
3. `appconfig.json` is created automatically on first launch with default values
4. `state.json` and log files are fully compatible

---

## Usage

### Graphical Interface

Launch `EasySave.exe` without arguments to open the main window.

**Main window toolbar:**

| Button | Action |
|---|---|
| ▶ Run All | Executes all configured backup jobs sequentially |
| + Add | Opens the job creation dialog |
| ✎ Edit | Opens the job editing dialog for the selected job |
| ✕ Delete | Deletes the selected job |
| ⚙ Settings | Opens the settings dialog |

Each job row shows a progress bar updated in real time. The ▶ button on each row executes that specific job.

### Command-Line Mode

The CLI mode is identical to v1.0 and v1.1. EasySave detects arguments at startup and runs headlessly without opening the main window:

```bash
EasySave.exe 1-3     # Execute jobs at positions 1, 2 and 3
EasySave.exe 1;3     # Execute jobs at positions 1 and 3
EasySave.exe 2       # Execute job at position 2
```

> Job positions in CLI mode correspond to the order of jobs in `config.json`.

### Settings

Open the settings dialog via the ⚙ button:

| Setting | Description |
|---|---|
| **Language** | UI language: `en` or `fr` |
| **Log format** | `JSON` or `XML` — applied immediately |
| **Business software** | Process name to detect (e.g. `calc`). Empty = detection disabled |
| **CryptoSoft path** | Full path to `CryptoSoft.exe`. Empty = encryption disabled |
| **Encrypted extensions** | List of extensions to encrypt after copy (e.g. `.pdf`, `.docx`) |

---

## Configuration

All data is stored under `%ProgramData%\EasySave\`.

| File | Path | Description |
|---|---|---|
| `config.json` | `%ProgramData%\EasySave\config.json` | Backup job definitions (Guid IDs) |
| `state.json` | `%ProgramData%\EasySave\state.json` | Real-time execution state |
| `appconfig.json` | `%ProgramData%\EasySave\appconfig.json` | User settings (encryption, business software) |
| Log files | `%ProgramData%\EasySave\logs\yyyy-MM-dd.{json\|xml}` | Daily transfer logs |
| `appsettings.json` | `EasySave.exe` directory | Optional path overrides |

### `appconfig.json` structure

```json
{
  "cryptoSoftPath": "C:\\Program Files\\CryptoSoft\\CryptoSoft.exe",
  "businessSoftwareName": "calc",
  "encryptedExtensions": [".pdf", ".docx", ".xlsx"],
  "logFormat": "JSON",
  "defaultLanguage": "en"
}
```

---

## Log Files

### JSON format

```json
{
  "timestamp": "2024-11-15 14:32:01.847",
  "jobName": "Backup_Docs",
  "sourceFile": "\\\\srv01\\documents\\report.pdf",
  "destFile": "\\\\srv02\\backup\\documents\\report.pdf",
  "fileSizeBytes": 204800,
  "transferTimeMs": 128,
  "encryptionTimeMs": 42,
  "isError": false,
  "isEncrypted": true
}
```

### XML format

```xml
<LogEntry>
  <Timestamp>2024-11-15 14:32:01.847</Timestamp>
  <JobName>Backup_Docs</JobName>
  <SourceFile>\\srv01\documents\report.pdf</SourceFile>
  <DestFile>\\srv02\backup\documents\report.pdf</DestFile>
  <FileSizeBytes>204800</FileSizeBytes>
  <TransferTimeMs>128</TransferTimeMs>
  <EncryptionTimeMs>42</EncryptionTimeMs>
  <IsError>false</IsError>
  <IsEncrypted>true</IsEncrypted>
</LogEntry>
```

---

## Architecture

```
┌────────────────────────────────────┐
│         EasySave.WPF               │  MVVM — Views, ViewModels, Commands
└───────────────┬────────────────────┘
                │ depends on
┌───────────────▼────────────────────┐
│     EasySave.Infrastructure        │  Repositories, Strategies, Adapters
│  + CryptoSoftAdapter               │  + BusinessSoftwareDetector
└───────────────┬────────────────────┘
                │ depends on
┌───────────────▼────────────────────┐   ┌─────────────────┐
│       EasySave.Core                │   │   EasyLog v2.0  │
│  (no external dependencies)        │   │  JSON + XML DLL │
└────────────────────────────────────┘   └─────────────────┘
```

Key design patterns:
- **MVVM** — WPF layer fully decoupled from business logic
- **Strategy** — `FullBackupStrategy` / `DifferentialBackupStrategy`
- **Adapter** — `CryptoSoftAdapter`, `EasyLogAdapter`, `BusinessSoftwareDetector`
- **Repository** — `JsonConfigRepository`, `JsonStateRepository`, `JsonAppConfigRepository`
- **Singleton** — `EasyLogWriter` (thread-safe)
- **Dependency Injection** — all dependencies injected via constructors

---

## Versioning

| Version | Type | Key change |
|---|---|---|
| v1.0.0 | Initial release | Console, JSON logs, 5 jobs, FR/EN |
| v1.1.0 | Patch | JSON + XML log format selection |
| **v2.0.0** | **Major** | **WPF GUI, unlimited jobs, CryptoSoft, business software detection** |

---

## Contributing

- All code, comments, and documentation must be in **English**
- Follow C# naming conventions (PascalCase / camelCase)
- No duplicated code — extract shared logic into methods
- XML documentation required on all public members
- The project supervisor must be invited as a GitHub collaborator

---

*EasySave v2.0.0 — ProSoft © 2024. All rights reserved.*
