# EasySave v2.0 — Technical Support Documentation

**ProSoft** | Internal document — Support team use only | Version 2.0.0

---

## 1. Application Overview

| Property | Value |
|---|---|
| Product name | EasySave |
| Version | 2.0.0 |
| Previous version | 1.1.0 |
| Type | Windows WPF Application |
| Runtime | .NET 8.0 Windows Desktop |
| Executable | `EasySave.exe` |
| Logging library | `EasyLog.dll` v2.0.0 |
| Maintenance coverage | 5/7 — Monday to Friday, 08:00–17:00 |

---

## 2. What Changed in v2.0

| Area | Change | Support impact |
|---|---|---|
| Interface | Console replaced by WPF GUI | No more console window — users interact via graphical menus |
| Backup jobs | Unlimited jobs, IDs are now Guid | `config.json` schema changed — **not compatible with v1.x** |
| Encryption | CryptoSoft integration | New `cryptoSoftPath` field in `appconfig.json` |
| Business software | Process detection before/during backup | New `businessSoftwareName` field in `appconfig.json` |
| Log entries | Two new fields: `encryptionTimeMs`, `isEncrypted` | Log parsing tools may need updating |
| Configuration | New `appconfig.json` file | Created automatically on first launch |
| CLI mode | Preserved — identical to v1.0/v1.1 | No change for automated tasks |

---

## 3. Minimum System Requirements

| Component | Minimum Requirement |
|---|---|
| Operating System | Windows 10 (64-bit) / Windows Server 2016 or later |
| .NET Runtime | .NET 8.0 **Windows Desktop** Runtime ([download](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| RAM | 512 MB available |
| Disk space | 50 MB for application + space for log files |
| Display | 1024×768 minimum resolution |
| Network | Required for UNC path access |
| CryptoSoft | Required only if file encryption is used |
| Permissions | Read on source directories; Write on target directories and `%ProgramData%\EasySave\` |

> **Critical:** WPF requires the **Windows Desktop Runtime**, not just the base .NET 8.0 runtime. Ensure the correct package is installed.

---

## 4. Default File Locations

| File | Default Path | Description |
|---|---|---|
| **Executable** | `<install_dir>\EasySave.exe` | Main application binary |
| **EasyLog DLL** | `<install_dir>\EasyLog.dll` | Logging library — must stay next to the executable |
| **Application settings** | `<install_dir>\appsettings.json` | Path overrides (optional) |
| **Job configuration** | `%ProgramData%\EasySave\config.json` | All backup job definitions |
| **App configuration** | `%ProgramData%\EasySave\appconfig.json` | User settings (encryption, business software) |
| **Real-time state** | `%ProgramData%\EasySave\state.json` | Live execution progress |
| **JSON log files** | `%ProgramData%\EasySave\logs\yyyy-MM-dd.json` | Daily logs in JSON format |
| **XML log files** | `%ProgramData%\EasySave\logs\yyyy-MM-dd.xml` | Daily logs in XML format |

---

## 5. Configuration Files

### 5.1 `config.json` — Backup Job Configuration

**Location:** `%ProgramData%\EasySave\config.json`  
**Status in v2.0:** **Schema changed** — job IDs are now `Guid` strings instead of integers.  
**Compatibility:** **Not compatible with v1.x** — do not overwrite existing v1.x config files.

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "Backup_Docs",
    "sourcePath": "\\\\srv01\\documents",
    "targetPath": "\\\\srv02\\backup\\documents",
    "type": "Full",
    "lastFullBackupDate": "2024-11-15T14:32:01.847"
  }
]
```

---

### 5.2 `appconfig.json` — User Application Configuration

**Location:** `%ProgramData%\EasySave\appconfig.json`  
**Status in v2.0:** **NEW** — created automatically on first launch with default values.

```json
{
  "cryptoSoftPath": "C:\\Program Files\\CryptoSoft\\CryptoSoft.exe",
  "businessSoftwareName": "calc",
  "encryptedExtensions": [".pdf", ".docx", ".xlsx"],
  "logFormat": "JSON",
  "defaultLanguage": "en"
}
```

| Field | Type | Description | Default |
|---|---|---|---|
| `cryptoSoftPath` | string | Full path to `CryptoSoft.exe`. Empty = encryption disabled | `""` |
| `businessSoftwareName` | string | Process name to detect. Empty = detection disabled | `""` |
| `encryptedExtensions` | array | Extensions to encrypt (with leading dot) | `[]` |
| `logFormat` | string | `"JSON"` or `"XML"` | `"JSON"` |
| `defaultLanguage` | string | `"en"` or `"fr"` | `"en"` |

---

### 5.3 `state.json` — Real-Time Execution State

**Location:** `%ProgramData%\EasySave\state.json`  
**Status in v2.0:** Unchanged structure — always JSON regardless of log format.

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

### 5.4 `appsettings.json` — Path Overrides

**Location:** Same directory as `EasySave.exe`  
**Status in v2.0:** New `AppConfigFilePath` field added.

```json
{
  "ConfigFilePath": "",
  "StateFilePath": "",
  "LogDirectory": "",
  "AppConfigFilePath": "",
  "DefaultLanguage": "en"
}
```

---

## 6. Log Files

### 6.1 JSON Format

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

### 6.2 XML Format

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

### 6.3 New Fields in v2.0

| Field | Values | Meaning |
|---|---|---|
| `encryptionTimeMs` | `0` | File not encrypted (extension not configured) |
| `encryptionTimeMs` | `> 0` | Encryption duration in milliseconds |
| `encryptionTimeMs` | `< 0` | CryptoSoft error code |
| `isEncrypted` | `true` | File was successfully encrypted |
| `isEncrypted` | `false` | Not encrypted or encryption failed |

### 6.4 Business Software Log Entries

When a job is blocked or interrupted by business software detection, a dedicated log entry is written:

```json
{
  "timestamp": "2024-11-15 14:35:00.001",
  "jobName": "Backup_Docs",
  "sourceFile": "BLOCKED: calc is running",
  "destFile": "",
  "fileSizeBytes": 0,
  "transferTimeMs": -1,
  "encryptionTimeMs": 0,
  "isError": true,
  "isEncrypted": false
}
```

---

## 7. Required Permissions

| Location | Required Permission |
|---|---|
| `%ProgramData%\EasySave\` | Read + Write |
| `%ProgramData%\EasySave\logs\` | Read + Write |
| `<install_dir>\appsettings.json` | Read + Write |
| Source directories | Read |
| Target directories | Read + Write + Create directories |
| Network shares (UNC) | Network access + share permissions |
| CryptoSoft executable | Execute |

---

## 8. Common Issues and Resolutions

| Symptom | Likely Cause | Resolution |
|---|---|---|
| Application crashes on startup | .NET 8.0 Windows Desktop Runtime not installed | Install the Desktop Runtime, not just the base runtime |
| Main window does not open | Application launched in CLI mode | Verify no arguments are passed at startup |
| Jobs missing after upgrade from v1.x | `config.json` schema mismatch (int vs Guid IDs) | Recreate jobs manually — v1.x config.json is not compatible |
| Encryption not applied | CryptoSoft path incorrect or executable missing | Verify path in Settings; confirm `CryptoSoft.exe` exists |
| Encryption error (`encryptionTimeMs < 0`) | CryptoSoft exited with non-zero code | Check CryptoSoft logs; verify file access permissions |
| Job blocked by business software | Process detected before launch | Close the business software or disable detection in Settings |
| Job interrupted mid-execution | Process detected during backup | Expected behaviour — check log for interruption entry |
| `appconfig.json` not created | Write permission denied on `%ProgramData%\EasySave\` | Grant write access to the application user |
| Log format not changing | Settings not saved | Click Save in the Settings dialog |

---

## 9. Upgrade from v1.1 — Checklist

- [ ] Replace `EasySave.exe` with the v2.0 binary
- [ ] Replace `EasyLog.dll` with the v2.0 binary
- [ ] **Do NOT copy v1.x `config.json`** — job schema is incompatible (int IDs vs Guid IDs)
- [ ] Recreate backup jobs manually via the Settings dialog
- [ ] `state.json` is compatible — no action needed
- [ ] Existing log files (`.json` / `.xml`) are compatible — no action needed
- [ ] `appconfig.json` is created automatically on first launch
- [ ] Configure CryptoSoft path and business software name in Settings if required
- [ ] Install .NET 8.0 **Windows Desktop Runtime** (not just the base runtime)

---

## 10. EasyLog.dll v2.0.0 Reference

| Property | Value |
|---|---|
| Assembly name | `EasyLog.dll` |
| Version | 2.0.0 |
| Public interface | `IEasyLogWriter` |
| Entry point | `LogWriterFactory.Create(logDirectory, format)` |
| Backward compatibility | `Write()` and `GetLogFilePath()` unchanged — v1.x consumers still compile |
| Breaking change | `LogEntryDto` has 2 new fields: `EncryptionTimeMs`, `IsEncrypted` |

---

*ProSoft — Internal support documentation — Do not distribute to end users*  
*Document version: 2.0 — November 2024*
