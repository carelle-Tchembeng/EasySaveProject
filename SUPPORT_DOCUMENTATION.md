# EasySave v1.0 — Technical Support Documentation

**ProSoft** | Internal document — Support team use only | Version 1.0.0

---

## 1. Application Overview

| Property | Value |
|---|---|
| Product name | EasySave |
| Version | 1.0.0 |
| Type | Windows Console Application |
| Runtime | .NET 8.0 |
| Executable | `EasySave.exe` |
| Logging library | `EasyLog.dll` v1.0.0 |
| Maintenance coverage | 5/7 — Monday to Friday, 08:00–17:00 |

---

## 2. Minimum System Requirements

| Component | Minimum Requirement |
|---|---|
| Operating System | Windows 10 (64-bit) / Windows Server 2016 or later |
| .NET Runtime | .NET 8.0 Runtime ([download](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| RAM | 512 MB available |
| Disk space | 50 MB for application + space for log files |
| Network | Required for UNC path access (network backup jobs) |
| Permissions | Read access to source directories; Write access to target directories and `%ProgramData%\EasySave\` |

---

## 3. Default File Locations

All application data is stored under `%ProgramData%\EasySave\`.  
On a standard Windows installation, this resolves to `C:\ProgramData\EasySave\`.

| File | Default Path | Description |
|---|---|---|
| **Executable** | `<install_dir>\EasySave.exe` | Main application binary |
| **EasyLog DLL** | `<install_dir>\EasyLog.dll` | Logging library (must stay next to the executable) |
| **Application settings** | `<install_dir>\appsettings.json` | Optional path overrides |
| **Job configuration** | `%ProgramData%\EasySave\config.json` | All configured backup jobs |
| **Real-time state** | `%ProgramData%\EasySave\state.json` | Live execution progress |
| **Daily log files** | `%ProgramData%\EasySave\logs\yyyy-MM-dd.json` | Transfer logs (one file per day) |

> `<install_dir>` = directory where `EasySave.exe` is deployed.  
> Paths can be overridden in `appsettings.json` (see Section 5).

---

## 4. Configuration Files

### 4.1 `config.json` — Backup Job Configuration

**Location:** `%ProgramData%\EasySave\config.json`  
**Created by:** EasySave on first job creation  
**Format:** JSON, UTF-8, indented

**Structure:**
```json
[
  {
    "id": 1,
    "name": "Backup_Docs",
    "sourcePath": "\\\\srv01\\documents",
    "targetPath": "\\\\srv02\\backup\\documents",
    "type": "Full",
    "lastFullBackupDate": "2024-11-15T14:32:01.847"
  },
  {
    "id": 2,
    "name": "Backup_Projects",
    "sourcePath": "C:\\Projects",
    "targetPath": "E:\\Backup\\Projects",
    "type": "Differential",
    "lastFullBackupDate": "2024-11-10T09:15:00.000"
  }
]
```

**Field descriptions:**

| Field | Type | Description |
|---|---|---|
| `id` | integer | Job index (1–5) |
| `name` | string | User-defined job name |
| `sourcePath` | string | Source directory (local, external, or UNC) |
| `targetPath` | string | Destination directory |
| `type` | string | `"Full"` or `"Differential"` |
| `lastFullBackupDate` | string / null | ISO 8601 datetime of last successful full backup. `null` if never run |

---

### 4.2 `state.json` — Real-Time Execution State

**Location:** `%ProgramData%\EasySave\state.json`  
**Updated by:** EasySave after every file transfer  
**Format:** JSON, UTF-8, indented  
**Write method:** Atomic (written to `.tmp` then renamed) — never partially written

**Structure:**
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
  },
  {
    "jobName": "Backup_Projects",
    "lastActionTime": "2024-11-15 13:00:00.000",
    "status": "Inactive",
    "totalFiles": 0,
    "totalSizeBytes": 0,
    "remainingFiles": 0,
    "remainingBytes": 0,
    "progressPercent": 0,
    "currentSourceFile": "",
    "currentDestFile": ""
  }
]
```

**Status values:**

| Status | Meaning |
|---|---|
| `Inactive` | Job is configured but not currently running |
| `Active` | Job is currently executing |
| `Completed` | Last execution finished successfully |
| `Error` | Last execution encountered at least one file copy error |

---

### 4.3 `appsettings.json` — Optional Path Overrides

**Location:** Same directory as `EasySave.exe`  
**Required:** No — application uses default paths if this file is absent or fields are empty

```json
{
  "ConfigFilePath": "",
  "StateFilePath": "",
  "LogDirectory": "",
  "DefaultLanguage": "en"
}
```

| Field | Description | Default if empty |
|---|---|---|
| `ConfigFilePath` | Override path for `config.json` | `%ProgramData%\EasySave\config.json` |
| `StateFilePath` | Override path for `state.json` | `%ProgramData%\EasySave\state.json` |
| `LogDirectory` | Override directory for log files | `%ProgramData%\EasySave\logs\` |
| `DefaultLanguage` | Fallback language if auto-detection fails | `"en"` |

---

## 5. Log Files

### 5.1 Daily Log File

**Location:** `%ProgramData%\EasySave\logs\yyyy-MM-dd.json`  
**Example:** `C:\ProgramData\EasySave\logs\2024-11-15.json`  
**Created by:** `EasyLog.dll` — one new file per calendar day  
**Format:** JSON entries appended sequentially, UTF-8, indented

**Single entry structure:**
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

**Key fields for support diagnosis:**

| Field | Description |
|---|---|
| `transferTimeMs` | Transfer time in ms. **Negative value = copy error** |
| `isError` | `true` if the file could not be copied |
| `sourceFile` | Full UNC path — use to locate the original file |
| `destFile` | Full UNC path — use to verify the copy destination |

---

## 6. Required Permissions

| Location | Required Permission |
|---|---|
| `%ProgramData%\EasySave\` | Read + Write (created automatically on first run) |
| `%ProgramData%\EasySave\logs\` | Read + Write (created automatically) |
| Source directories | Read |
| Target directories | Read + Write + Create directories |
| Network shares (UNC) | Network access + appropriate share permissions |

> If `%ProgramData%\EasySave\` cannot be created, verify that the user running `EasySave.exe` has write access to `C:\ProgramData\`.

---

## 7. Common Issues and Resolutions

| Symptom | Likely Cause | Resolution |
|---|---|---|
| Application crashes on startup | .NET 8.0 Runtime not installed | Install .NET 8.0 from microsoft.com/dotnet |
| `config.json` not found | First launch — normal behaviour | No action needed; file is created on first job save |
| Job status stuck at `Active` in `state.json` | Application was force-closed during a backup | Restart EasySave — `state.json` is reset automatically on startup |
| Log file not created | `EasyLog.dll` missing from install directory | Ensure `EasyLog.dll` is present next to `EasySave.exe` |
| "Source or target path not found" | Path does not exist or network share is unreachable | Verify path accessibility from the machine running EasySave |
| Differential backup copies all files | `lastFullBackupDate` is null in `config.json` | Expected behaviour — a full backup is performed automatically |
| Log files growing large | Normal accumulation over time | Implement a log rotation policy at the OS level (e.g. scheduled deletion of files older than 90 days) |

---

## 8. Deployment Checklist

Before deploying EasySave on a customer server, verify the following:

- [ ] .NET 8.0 Runtime is installed on the target machine
- [ ] `EasySave.exe` and `EasyLog.dll` are in the same directory
- [ ] `appsettings.json` is configured if default paths are not suitable
- [ ] The application user has **write access** to `%ProgramData%\EasySave\`
- [ ] Source and target directories are **accessible** from the server
- [ ] For network paths, share permissions allow the application user to read/write
- [ ] A **maintenance task** is scheduled if CLI mode is used (Windows Task Scheduler)

---

## 9. EasyLog.dll Reference

`EasyLog.dll` is a standalone reusable library. It must remain in the same directory as `EasySave.exe`.

| Property | Value |
|---|---|
| Assembly name | `EasyLog.dll` |
| Version | 1.0.0 |
| Public interface | `IEasyLogWriter` |
| Entry point | `LogWriterFactory.Create(logDirectory)` |
| Thread safety | Yes — singleton with lock |
| Backward compatibility | Guaranteed — v1.x interface will not change |

---

*ProSoft — Internal support documentation — Do not distribute to end users*  
*Document version: 1.0 — November 2024*
