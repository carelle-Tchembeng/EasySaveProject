# EasySave v2.0 — User Manual

**ProSoft** | Version 2.0.0 | November 2024

---

## Getting Started

Launch `EasySave.exe`. The main window opens automatically with your configured backup jobs.

---

## Main Window

The main window displays all configured backup jobs in a table with real-time progress bars.

**Toolbar buttons:**

| Button | Action |
|---|---|
| **▶ Run All** | Executes all backup jobs sequentially |
| **+ Add** | Opens the job creation form |
| **✎ Edit** | Opens the editing form for the selected job |
| **✕ Delete** | Permanently deletes the selected job |
| **⚙ Settings** | Opens the application settings |

Select a job by clicking its row, then use the toolbar or click ▶ on the row to run it individually.

---

## Creating or Editing a Backup Job

Click **+ Add** or select a job and click **✎ Edit**.

| Field | Description |
|---|---|
| **Job name** | A descriptive label (e.g. `Backup_Docs`) |
| **Source directory** | Folder to back up — local, external drive, or network UNC path |
| **Target directory** | Destination folder for copied files |
| **Backup type** | **Full** copies everything · **Differential** copies only changed files |

> The number of backup jobs is **unlimited** in v2.0.

---

## Backup Types

| Type | Behaviour |
|---|---|
| **Full** | Copies every file on each execution |
| **Differential** | Copies only files modified since the last full backup |

If a differential job has never had a full backup, a full backup runs automatically on the first execution.

---

## Command-Line Execution

EasySave can be run from a terminal or task scheduler — the main window does not open:

```
EasySave.exe 1-3     → Execute jobs 1, 2 and 3 (range)
EasySave.exe 1;3     → Execute jobs 1 and 3 (list)
EasySave.exe 2       → Execute job 2 only
```

---

## Settings

Click **⚙ Settings** to configure application behaviour.

### Language

Select `en` (English) or `fr` (French). Applied immediately.

### Log File Format

| Choice | Output |
|---|---|
| **JSON** | `2024-11-15.json` — default format |
| **XML** | `2024-11-15.xml` — required by some clients |

### Business Software Detection

Enter the **process name** of the software to monitor (e.g. `calc` for Calculator).  
Leave empty to disable detection.

- If the software is **running when a job starts**: the backup is blocked and logged.
- If detected **during a backup**: the current file finishes copying, then the backup stops and the interruption is logged.

### CryptoSoft Encryption

Enter the **full path** to `CryptoSoft.exe`.  
Add file extensions to the **Encrypted extensions** list (e.g. `.pdf`, `.docx`).

Files with matching extensions are encrypted after copying. The encryption time appears in the log:

| `EncryptionTimeMs` | Meaning |
|---|---|
| `0` | Not encrypted |
| `> 0` | Encrypted — duration in ms |
| `< 0` | CryptoSoft error |

Click **Save** to apply and persist all settings.

---

## Supported Path Formats

| Type | Example |
|---|---|
| Local drive | `C:\Users\John\Documents` |
| External drive | `E:\Backup` |
| Network (UNC) | `\\server01\shared\docs` |

---

## Log Files

EasySave generates two files automatically:

- **Daily log** — `%ProgramData%\EasySave\logs\yyyy-MM-dd.{json|xml}`  
  One entry per file: source, destination, size, transfer time, encryption time.

- **State file** — `%ProgramData%\EasySave\state.json`  
  Updated in real time with progress of all running jobs.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Backup blocked at start | Business software is running — close it or disable detection in Settings |
| Backup stops mid-execution | Business software was detected — check the log for the interruption entry |
| Encryption not working | Verify CryptoSoft path in Settings and ensure the file exists |
| Job shows Error status | Check the daily log for the file that caused the error |
| Log files in wrong format | Open Settings and re-select the desired format |

---

*For technical support, contact ProSoft support team — Maintenance contract: 5/7 8h–17h*
