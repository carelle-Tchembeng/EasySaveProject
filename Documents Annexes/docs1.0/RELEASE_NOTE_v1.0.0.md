# Release Note — EasySave v1.0.0

**ProSoft** | Release date: November 2024

---

## Overview

This is the **initial release** of EasySave, a file backup console application developed by ProSoft.  
EasySave v1.0.0 delivers the full feature set defined in the v1.0 specification.

---

## What's New in v1.0.0

### Core Features

- **Backup job management** — Create, edit, delete, and list up to 5 backup jobs  
  Each job is defined by a name, source path, target path, and backup type.

- **Full backup** — Copies all files and subdirectories from source to target recursively.  
  The entire directory tree is mirrored on each execution.

- **Differential backup** — Copies only files modified since the last successful full backup.  
  Automatically falls back to a full backup if no previous full backup exists.

- **Sequential execution** — Jobs can be executed one at a time or all at once in sequence.

### Interface

- **Interactive console menu** — Full bilingual menu interface (French / English).  
  Language is auto-detected from the OS and confirmed at startup.

- **Command-line mode** — Execute specific jobs directly from a terminal or task scheduler:
  - `EasySave.exe 1-3` — executes jobs 1, 2 and 3 (range)
  - `EasySave.exe 1;3` — executes jobs 1 and 3 (list)

### File Support

- Local drives (`C:\`, `D:\`)
- External drives (USB, portable disks)
- UNC network paths (`\\server\share\`)
- All source files and subdirectories are copied recursively

### Logging — EasyLog.dll v1.0.0

- **Daily log file** — One JSON file per day (`yyyy-MM-dd.json`) written to `%ProgramData%\EasySave\logs\`  
  Each entry records: timestamp, job name, source path (UNC), destination path (UNC), file size, transfer time (ms).  
  Transfer time is negative if a copy error occurred.

- **EasyLog.dll** is delivered as a standalone reusable library.  
  It can be integrated into other ProSoft products independently of EasySave.

### State Tracking

- **Real-time state file** (`state.json`) — Updated after every file transfer.  
  Contains current status, progress percentage, files remaining, and active file paths for all configured jobs.  
  Written atomically to prevent data corruption on unexpected shutdown.

### Configuration

- All data stored under `%ProgramData%\EasySave\` — no temp paths used.
- Job configuration persisted in `config.json` (JSON, UTF-8, indented).
- Optional `appsettings.json` allows path overrides for server deployments.
- Stale `Active` states in `state.json` are automatically cleared on startup.

---

## Technical Specifications

| Property | Value |
|---|---|
| Target framework | .NET 8.0 |
| Language | C# |
| Architecture | 4-layer — Core / Infrastructure / EasyLog / ConsoleApp |
| Design patterns | Strategy, Repository, Adapter, Singleton, Dependency Injection |
| Build tool | Visual Studio 2022 |
| Version control | GitHub |

---

## Known Limitations

| Limitation | Detail |
|---|---|
| Maximum backup jobs | 5 jobs per installation |
| Execution mode | Sequential only — no parallel execution in v1.0 |
| Platform | Windows only — Linux/macOS not supported |
| Interface | Console only — no graphical interface in v1.0 |
| Log rotation | No built-in log rotation — manage log files at OS level |

---

## Bug Fixes

None — this is the initial release.

---

## Upgrade Notes

No previous version exists. This is a fresh installation.

To install:
1. Copy `EasySave.exe` and `EasyLog.dll` to the target directory
2. Ensure .NET 8.0 Runtime is installed
3. Run `EasySave.exe` — all directories are created automatically on first launch

---

## Compatibility

| Component | Compatibility |
|---|---|
| EasyLog.dll | v1.0.0 — all future v1.x versions of EasyLog will remain backward compatible with this release |
| config.json | Stable schema — upgrades will not break existing configuration files |
| state.json | Stable schema — cleared automatically on startup |

---

## Roadmap — Planned for v2.0

The following features are planned for the next major version:

- Graphical user interface (WPF) based on the **MVVM architecture**
- The Core layer (business logic) will be reused without modification
- Additional language support

---

*EasySave v1.0.0 — ProSoft © 2024. All rights reserved.*  
*Maintenance contract: 5/7, 08:00–17:00 — Annual renewal with SYNTEC index revaluation.*
