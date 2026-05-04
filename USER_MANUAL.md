# EasySave v1.0 — User Manual

**ProSoft** | Version 1.0.0 | November 2024

---

## Getting Started

Launch `EasySave.exe` from your terminal. Select your language (`fr` / `en`) when prompted. The main menu appears automatically.

---

## Main Menu

| Option | Action |
|---|---|
| `1` | List all configured backup jobs |
| `2` | Create a new backup job |
| `3` | Edit an existing backup job |
| `4` | Delete a backup job |
| `5` | Execute a single backup job |
| `6` | Execute all backup jobs |
| `7` | Quit EasySave |

---

## Creating a Backup Job

Select option `2`. You will be prompted to enter:

- **Job name** — a label to identify the backup (e.g. `Backup_Docs`)
- **Source directory** — the folder to back up (local, external drive, or network UNC path)
- **Target directory** — the destination folder for the copied files
- **Backup type** — enter `1` for Full, `2` for Differential

> A maximum of **5 backup jobs** can be configured at any time.

---

## Backup Types

| Type | Behaviour |
|---|---|
| **Full** | Copies every file from source to target on each execution |
| **Differential** | Copies only files modified since the last full backup |

> If a differential job has never had a full backup, a full backup is performed automatically on first run.

---

## Executing a Backup

**Single job** — Select option `5`, then enter the job number (1–5).  
**All jobs** — Select option `6` to run all configured jobs sequentially.

Progress is displayed in real time during execution.

---

## Command-Line Execution

EasySave can be run directly from a terminal or task scheduler without the interactive menu:

```
EasySave.exe 1-3     → Execute jobs 1, 2 and 3 (range)
EasySave.exe 1;3     → Execute jobs 1 and 3 (list)
EasySave.exe 2       → Execute job 2 only
```

---

## Supported Path Formats

| Type | Example |
|---|---|
| Local drive | `C:\Users\John\Documents` |
| External drive | `E:\Backup` |
| Network (UNC) | `\\server01\shared\docs` |

---

## Log Files

EasySave automatically generates two files after each execution:

- **Daily log** — `%ProgramData%\EasySave\logs\yyyy-MM-dd.json`  
  Records every file transfer: source, destination, size, transfer time, and errors.

- **State file** — `%ProgramData%\EasySave\state.json`  
  Updated in real time with the progress of all running jobs.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| "Source or target path not found" | Verify the path exists and is accessible before saving the job |
| "Limit reached: maximum 5 jobs" | Delete an existing job before creating a new one |
| Job shows `Error` status | Check the daily log file for the affected file details |
| Application displays wrong language | Restart and select the correct language at the prompt |

---

*For technical support, contact ProSoft support team — Maintenance contract: 5/7 8h–17h*
