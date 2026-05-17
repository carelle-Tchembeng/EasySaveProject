# **EasySave v1.1**

Backup software developed by **ProSoft** — Console application built with .NET 8.0

## **Table of Contents**

* [Overview](#bookmark=id.8q3vigybt8x8)  
* [Features](#bookmark=id.1fg90cts9r0m)  
* [Prerequisites](#bookmark=id.lisefzanf0zn)  
* [Project Structure](#bookmark=id.6n36mb1d1acj)  
* [Installation](#bookmark=id.u032aal1zs4n)  
* [Usage](#bookmark=id.r4tgg38xu44n)  
  * [Interactive Mode](#bookmark=id.60o3ux4i5tzo)  
  * [Command-Line Mode](#bookmark=id.s34c0bcl2ekg)  
* [Configuration](#bookmark=id.aq56n9owbbr5)  
* [Log Files](#bookmark=id.8bk2o2shzajf)  
* [Architecture](#bookmark=id.2wdhnhbpub3c)  
* [Contributing](#bookmark=id.ex3mo3lanzc)

## **Overview**

EasySave is a file backup solution that allows users to define up to **5 backup jobs**, each configured with a source directory, a target directory, and a backup type (full or differential). It runs as a Windows console application and supports both interactive and command-line execution modes.

EasySave is part of the **ProSoft software suite**.

|  |  |
| :---- | :---- |
| **Version** | 1.1.0 |
| **License** | Proprietary — ProSoft |
| **Platform** | Windows |
| **Runtime** | .NET 8.0 |
| **Language** | C# |

## **Features**

* Create, edit, delete, and list up to **5 backup jobs**  
* **Full backup** — copies all files from source to target  
* **Differential backup** — copies only files modified since the last full backup  
* Supports **local drives**, **external drives**, and **UNC network paths**  
* **Real-time state file** (state.json) updated after each file transfer  
* **Daily log file** (yyyy-MM-dd.json or yyyy-MM-dd.xml) written via the EasyLog.dll library  
* **Log formats** — choose between JSON and XML for daily logs dynamically via the settings menu  
* **Bilingual interface** — French and English  
* **CLI mode** — execute backup jobs directly from a script or task scheduler  
* All configuration and log files stored in %ProgramData%\\EasySave\\ to support client/server environments

## **Prerequisites**

| Requirement | Version |
| :---- | :---- |
| Operating System | Windows 10 / Windows Server 2016 or later |
| .NET Runtime | [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Disk space | 50 MB minimum |
| RAM | 512 MB minimum |
| Visual Studio (dev only) | 2022 or later |

## **Project Structure**

EasySave/  
├── EasySave.sln                     \# Visual Studio solution  
├── EasyLog/                         \# Logging DLL (reusable library)  
│   ├── IEasyLogWriter.cs  
│   ├── LogWriterOptions.cs  
│   ├── DTOs/LogEntryDto.cs  
│   ├── Factory/LogWriterFactory.cs  
│   ├── Formatters/              \# LogFormatter.cs, JsonLogFormatter.cs, XmlLogFormatter.cs  
│   ├── Helpers/LogFileNamer.cs  
│   └── Writers/EasyLogWriter.cs  
├── EasySave.Core/                   \# Business logic — no external dependencies  
│   ├── Entities/BackupJob.cs  
│   ├── Enums/BackupType.cs  
│   ├── Enums/BackupStatus.cs  
│   ├── Interfaces/                  \# IBackupStrategy, IFileSystem, ILogger, ...  
│   ├── Services/BackupService.cs  
│   ├── Services/JobManager.cs  
│   └── ValueObjects/               \# LogEntry, BackupProgress  
├── EasySave.Infrastructure/         \# Implementations (file system, JSON/XML, strategies)  
│   ├── Configuration/AppSettings.cs  
│   ├── FileSystem/WindowsFileSystem.cs  
│   ├── Helpers/PathHelper.cs  
│   ├── Logging/EasyLogAdapter.cs  
│   ├── Repositories/               \# JsonConfigRepository, JsonStateRepository  
│   └── Strategies/                 \# FullBackupStrategy, DifferentialBackupStrategy  
└── EasySave.ConsoleApp/             \# Console UI, menu, CLI parser  
    ├── Program.cs  
    ├── App.cs  
    ├── Controllers/MenuController.cs  
    ├── DI/ServiceContainer.cs  
    ├── Localization/               \# ILocalizer, ResourceLocalizer (FR/EN)  
    ├── Parsers/CliArgumentParser.cs  
    └── Views/                      \# IConsoleView, ConsoleView, BackupJobFormData

## **Installation**

### **Option 1 — Clone and build from source**

\# 1\. Clone the repository  
git clone \[https://github.com/ProSoft/EasySave.git\](https://github.com/ProSoft/EasySave.git)  
cd EasySave

\# 2\. Restore dependencies and build  
dotnet build EasySave.sln \--configuration Release

\# 3\. Run the application  
cd EasySave.ConsoleApp/bin/Release/net8.0  
EasySave.exe

### **Option 2 — Open in Visual Studio 2022**

1. Open EasySave.sln in Visual Studio 2022  
2. Set EasySave.ConsoleApp as the **Startup Project**  
3. Press F5 to build and run

**Note:** The application requires .NET 8.0 Runtime. Download it at https://dotnet.microsoft.com/download/dotnet/8.0

## **Usage**

### **Interactive Mode**

Launch EasySave.exe without arguments to enter the interactive menu:

EasySave.exe

You will be prompted to select a language (fr / en), then the main menu is displayed:

\=== EasySave v1.1 \===

\--- MAIN MENU \--- 
1\. List backup jobs  
2\. Create a backup job  
3\. Edit a backup job  
4\. Delete a backup job  
5\. Execute a backup job  
6\. Execute all backup jobs  
7\. General settings  
8\. Quit

Your choice:

### **Command-Line Mode**

EasySave can be executed directly from a terminal or a task scheduler with arguments.

#### **Execute a range of jobs**

EasySave.exe 1-3

Executes backup jobs 1, 2, and 3 sequentially.

#### **Execute a specific list of jobs**

EasySave.exe 1;3

Executes backup jobs 1 and 3 (job 2 is skipped).

#### **Execute a single job**

EasySave.exe 2

Executes backup job 2 only.

Job indices correspond to the IDs shown in the job list (1 to 5).

## **Configuration**

EasySave stores all its files under %ProgramData%\\EasySave\\ (e.g. C:\\ProgramData\\EasySave\\).

| File | Path | Description |
| :---- | :---- | :---- |
| config.json | %ProgramData%\\EasySave\\config.json | Backup job definitions |
| state.json | %ProgramData%\\EasySave\\state.json | Real-time execution state |
| Log files | %ProgramData%\\EasySave\\logs\\yyyy-MM-dd.(json|xml) | Daily transfer logs |
| Settings | %ProgramData%\\EasySave\\settings.json | Global application settings and overrides |

These paths and options can be configured interactively via **Option 7** in the main menu, or by directly editing settings.json (automatically created on first run):

{  
  "ConfigFilePath": "\\\\\\\\server\\\\share\\\\EasySave\\\\config.json",  
  "StateFilePath":  "\\\\\\\\server\\\\share\\\\EasySave\\\\state.json",  
  "LogDirectory":   "\\\\\\\\server\\\\share\\\\EasySave\\\\logs",  
  "LogFormat":      "Xml",  
  "Language":       "en"  
}

## **Log Files**

### **Daily log (yyyy-MM-dd.json or yyyy-MM-dd.xml)**

A new log file is created each day in %ProgramData%\\EasySave\\logs\\. Depending on your configuration in settings.json, it can be formatted in JSON or XML. Each entry records one file transfer:

**JSON Example:**

{  
  "timestamp": "2024-11-15 14:32:01.847",  
  "jobName": "Backup\_Docs",  
  "sourceFile": "\\\\\\\\srv01\\\\documents\\\\report.pdf",  
  "destFile": "\\\\\\\\srv02\\\\backup\\\\documents\\\\report.pdf",  
  "fileSizeBytes": 204800,  
  "transferTimeMs": 128,  
  "isError": false  
}

A negative transferTimeMs value indicates a copy error.

### **Real-time state (state.json)**

Updated after every file transfer. Contains the current status of all jobs:

\[  
  {  
    "jobName": "Backup\_Docs",  
    "lastActionTime": "2024-11-15 14:32:01.847",  
    "status": "Active",  
    "totalFiles": 120,  
    "totalSizeBytes": 524288000,  
    "remainingFiles": 45,  
    "remainingBytes": 196608000,  
    "progressPercent": 62,  
    "currentSourceFile": "\\\\\\\\srv01\\\\documents\\\\report.pdf",  
    "currentDestFile": "\\\\\\\\srv02\\\\backup\\\\documents\\\\report.pdf"  
  }  
\]

## **Architecture**

EasySave follows a **layered architecture** with strict dependency rules:

┌─────────────────────────────────┐  
│       EasySave.ConsoleApp       │  UI, menu, CLI parser  
└──────────────┬──────────────────┘  
               │ depends on  
┌──────────────▼──────────────────┐  
│    EasySave.Infrastructure      │  File system, configuration, strategies  
└──────────────┬──────────────────┘  
               │ depends on  
┌──────────────▼──────────────────┐     ┌─────────────┐  
│       EasySave.Core             │     │   EasyLog   │  
│  (no external dependencies)     │     │    .dll     │  
└─────────────────────────────────┘     └─────────────┘

Key design patterns applied:

* **Strategy** — FullBackupStrategy / DifferentialBackupStrategy  
* **Repository** — JsonConfigRepository / JsonStateRepository  
* **Adapter** — EasyLogAdapter bridges Core and EasyLog DLL  
* **Singleton** — EasyLogWriter for thread-safe file writes  
* **Dependency Injection** — all dependencies injected via constructors

## **Contributing**

This project is managed with **Git** and hosted on **GitHub**.

The project supervisor must be invited as a collaborator to follow development progress.

### **Branch strategy**

| Branch | Purpose |
| :---- | :---- |
| main | Stable releases only |
| develop | Active development |

### **Code standards**

* All code, comments, and documentation **must be in English**  
* Follow **C\# naming conventions** (PascalCase for types/methods, camelCase for variables)  
* No duplicated code — extract shared logic into methods or services  
* Functions must remain short and focused (single responsibility)  
* XML documentation (\<summary\>) required on all public members

*EasySave v1.1 — ProSoft © 2024-2026. All rights reserved.*