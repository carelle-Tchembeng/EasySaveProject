# EasySave v1.0

> Backup software developed by **ProSoft** — Console application built with .NET 8.0

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Usage](#usage)
  - [Interactive Mode](#interactive-mode)
  - [Command-Line Mode](#command-line-mode)
- [Configuration](#configuration)
- [Log Files](#log-files)
- [Architecture](#architecture)
- [Contributing](#contributing)

---

## Overview

EasySave is a file backup solution that allows users to define up to **5 backup jobs**, each configured with a source directory, a target directory, and a backup type (full or differential). It runs as a Windows console application and supports both interactive and command-line execution modes.

EasySave is part of the **ProSoft software suite**.

| | |
|---|---|
| **Version** | 1.0.0 |
| **License** | Proprietary — ProSoft |
| **Platform** | Windows |
| **Runtime** | .NET 8.0 |
| **Language** | C# |

---

## Features

- Create, edit, delete, and list up to **5 backup jobs**
- **Full backup** — copies all files from source to target
- **Differential backup** — copies only files modified since the last full backup
- Supports **local drives**, **external drives**, and **UNC network paths**
- **Real-time state file** (`state.json`) updated after each file transfer
- **Daily log file** (`yyyy-MM-dd.json`) written via the `EasyLog.dll` library
- **Bilingual interface** — French and English (auto-detected from OS language)
- **CLI mode** — execute backup jobs directly from a script or task scheduler
- All configuration and log files stored in `%ProgramData%\EasySave\`

---

## Prerequisites

| Requirement | Version |
|---|---|
| Operating System | Windows 10 / Windows Server 2016 or later |
| .NET Runtime | [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Disk space | 50 MB minimum |
| RAM | 512 MB minimum |
| Visual Studio (dev only) | 2022 or later |

---

## Project Structure

```
EasySave/
├── EasySave.sln                     # Visual Studio solution
├── EasyLog/                         # Logging DLL (reusable library)
│   ├── IEasyLogWriter.cs
│   ├── LogWriterOptions.cs
│   ├── DTOs/LogEntryDto.cs
│   ├── Factory/LogWriterFactory.cs
│   ├── Formatters/LogFormatter.cs
│   ├── Helpers/LogFileNamer.cs
│   └── Writers/EasyLogWriter.cs
├── EasySave.Core/                   # Business logic — no external dependencies
│   ├── Entities/BackupJob.cs
│   ├── Enums/BackupType.cs
│   ├── Enums/BackupStatus.cs
│   ├── Interfaces/                  # IBackupStrategy, IFileSystem, ILogger, ...
│   ├── Services/BackupService.cs
│   ├── Services/JobManager.cs
│   └── ValueObjects/               # LogEntry, BackupProgress
├── EasySave.Infrastructure/         # Implementations (file system, JSON, strategies)
│   ├── Configuration/AppSettings.cs
│   ├── FileSystem/WindowsFileSystem.cs
│   ├── Helpers/PathHelper.cs
│   ├── Logging/EasyLogAdapter.cs
│   ├── Repositories/               # JsonConfigRepository, JsonStateRepository
│   └── Strategies/                 # FullBackupStrategy, DifferentialBackupStrategy
└── EasySave.ConsoleApp/             # Console UI, menu, CLI parser
    ├── Program.cs
    ├── App.cs
    ├── Controllers/MenuController.cs
    ├── DI/ServiceContainer.cs
    ├── Localization/               # ILocalizer, ResourceLocalizer (FR/EN)
    ├── Parsers/CliArgumentParser.cs
    └── Views/                      # IConsoleView, ConsoleView, BackupJobFormData
```

---

## Installation

### Option 1 — Clone and build from source

```bash
# 1. Clone the repository
git clone https://github.com/ProSoft/EasySave.git
cd EasySave

# 2. Restore dependencies and build
dotnet build EasySave.sln --configuration Release

# 3. Run the application
cd EasySave.ConsoleApp/bin/Release/net8.0
EasySave.exe
```

### Option 2 — Open in Visual Studio 2022

1. Open `EasySave.sln` in Visual Studio 2022
2. Set `EasySave.ConsoleApp` as the **Startup Project**
3. Press `F5` to build and run

> **Note:** The application requires .NET 8.0 Runtime.  
> Download it at https://dotnet.microsoft.com/download/dotnet/8.0

---

## Usage

### Interactive Mode

Launch `EasySave.exe` without arguments to enter the interactive menu:

```
EasySave.exe
```

You will be prompted to select a language (`fr` / `en`), then the main menu is displayed:

```
=== EasySave v1.0 ===

--- MAIN MENU ---
1. List backup jobs
2. Create a backup job
3. Edit a backup job
4. Delete a backup job
5. Execute a backup job
6. Execute all backup jobs
7. Quit

Your choice:
```

### Command-Line Mode

EasySave can be executed directly from a terminal or a task scheduler with arguments.

#### Execute a range of jobs

```bash
EasySave.exe 1-3
```
Executes backup jobs 1, 2, and 3 sequentially.

#### Execute a specific list of jobs

```bash
EasySave.exe 1;3
```
Executes backup jobs 1 and 3 (job 2 is skipped).

#### Execute a single job

```bash
EasySave.exe 2
```
Executes backup job 2 only.

> Job indices correspond to the IDs shown in the job list (1 to 5).

---

## Configuration

EasySave stores all its files under `%ProgramData%\EasySave\` (e.g. `C:\ProgramData\EasySave\`).

| File | Path | Description |
|---|---|---|
| `config.json` | `%ProgramData%\EasySave\config.json` | Backup job definitions |
| `state.json` | `%ProgramData%\EasySave\state.json` | Real-time execution state |
| Log files | `%ProgramData%\EasySave\logs\yyyy-MM-dd.json` | Daily transfer logs |
| Settings | `EasySave.exe` directory — `appsettings.json` | Optional path overrides |

These paths can be overridden in `appsettings.json` (located next to `EasySave.exe`):

```json
{
  "ConfigFilePath": "\\\\server\\share\\EasySave\\config.json",
  "StateFilePath":  "\\\\server\\share\\EasySave\\state.json",
  "LogDirectory":   "\\\\server\\share\\EasySave\\logs",
  "DefaultLanguage": "en"
}
```

---

## Log Files

### Daily log (`yyyy-MM-dd.json`)

A new log file is created each day in `%ProgramData%\EasySave\logs\`.  
Each entry records one file transfer:

```json
{
  "timestamp": "2024-11-15 14:32:01.847",
  "jobName": "Backup_Docs",
  "sourceFile": "\\\\srv01\\documents\\report.pdf",
  "destFile": "\\\\srv02\\backup\\documents\\report.pdf",
  "fileSizeBytes": 204800,
  "transferTimeMs": 128,
  "isError": false
}
```

> A negative `transferTimeMs` value indicates a copy error.

### Real-time state (`state.json`)

Updated after every file transfer. Contains the current status of all jobs:

```json
[
  {
    "jobName": "Backup_Docs",
    "lastActionTime": "2024-11-15 14:32:01.847",
    "status": "Active",
    "totalFiles": 120,
    "totalSizeBytes": 524288000,
    "remainingFiles": 45,
    "remainingBytes": 196608000,
    "progressPercent": 62,
    "currentSourceFile": "\\\\srv01\\documents\\report.pdf",
    "currentDestFile": "\\\\srv02\\backup\\documents\\report.pdf"
  }
]
```

---

## Architecture

EasySave follows a **layered architecture** with strict dependency rules:

```
┌─────────────────────────────────┐
│       EasySave.ConsoleApp       │  UI, menu, CLI parser
└──────────────┬──────────────────┘
               │ depends on
┌──────────────▼──────────────────┐
│    EasySave.Infrastructure      │  File system, JSON, strategies
└──────────────┬──────────────────┘
               │ depends on
┌──────────────▼──────────────────┐     ┌─────────────┐
│       EasySave.Core             │     │   EasyLog   │
│  (no external dependencies)     │     │    .dll     │
└─────────────────────────────────┘     └─────────────┘
```

Key design patterns applied:
- **Strategy** — `FullBackupStrategy` / `DifferentialBackupStrategy`
- **Repository** — `JsonConfigRepository` / `JsonStateRepository`
- **Adapter** — `EasyLogAdapter` bridges Core and EasyLog DLL
- **Singleton** — `EasyLogWriter` for thread-safe file writes
- **Dependency Injection** — all dependencies injected via constructors

---

## Contributing

This project is managed with **Git** and hosted on **GitHub**.  
The project supervisor must be invited as a collaborator to follow development progress.

### Branch strategy

| Branch | Purpose |
|---|---|
| `main` | Stable releases only |
| `develop` | Active development |

### Code standards

- All code, comments, and documentation **must be in English**
- Follow **C# naming conventions** (PascalCase for types/methods, camelCase for variables)
- No duplicated code — extract shared logic into methods or services
- Functions must remain short and focused (single responsibility)
- XML documentation (`<summary>`) required on all public members

---

*EasySave v1.0 — ProSoft © 2024. All rights reserved.*
