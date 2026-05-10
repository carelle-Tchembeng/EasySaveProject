# **EasySave v1.1 — Technical Support Documentation**

**ProSoft** | Internal document — Support team use only | Version 1.1.0

## **1\. Application Overview**

| Property | Value |
| :---- | :---- |
| Product name | EasySave |
| Version | 1.1.0 |
| Type | Windows Console Application |
| Runtime | .NET 8.0 |
| Executable | EasySave.exe |
| Logging library | EasyLog.dll v1.1.0 |
| Maintenance coverage | 5/7 — Monday to Friday, 08:00–17:00 |

## **2\. Minimum System Requirements**

| Component | Minimum Requirement |
| :---- | :---- |
| Operating System | Windows 10 (64-bit) / Windows Server 2016 or later |
| .NET Runtime | .NET 8.0 Runtime ([download](https://dotnet.microsoft.com/download/dotnet/8.0)) |
| RAM | 512 MB available |
| Disk space | 50 MB for application \+ space for log files |
| Network | Required for UNC path access (network backup jobs) |
| Permissions | Read access to source directories; Write access to target directories and %ProgramData%\\EasySave\\ |

## **3\. Default File Locations**

All application data is stored under %ProgramData%\\EasySave\\.

On a standard Windows installation, this resolves to C:\\ProgramData\\EasySave\\.

| File | Default Path | Description |
| :---- | :---- | :---- |
| **Executable** | \<install\_dir\>\\EasySave.exe | Main application binary |
| **EasyLog DLL** | \<install\_dir\>\\EasyLog.dll | Logging library (must stay next to the executable) |
| **Application settings** | %ProgramData%\\EasySave\\settings.json | Global application settings and overrides |
| **Job configuration** | %ProgramData%\\EasySave\\config.json | All configured backup jobs |
| **Real-time state** | %ProgramData%\\EasySave\\state.json | Live execution progress |
| **Daily log files** | %ProgramData%\\EasySave\\logs\\yyyy-MM-dd.(json|xml) | Transfer logs (one file per day) |

\<install\_dir\> \= directory where EasySave.exe is deployed.

Paths can be overridden in settings.json (see Section 4.3).

## **4\. Configuration Files**

### **4.1 config.json — Backup Job Configuration**

**Location:** %ProgramData%\\EasySave\\config.json

**Created by:** EasySave on first job creation

**Format:** JSON, UTF-8, indented

**Structure:**

\[  
  {  
    "id": 1,  
    "name": "Backup\_Docs",  
    "sourcePath": "\\\\\\\\srv01\\\\documents",  
    "targetPath": "\\\\\\\\srv02\\\\backup\\\\documents",  
    "type": "Full",  
    "lastFullBackupDate": "2024-11-15T14:32:01.847"  
  }  
\]

### **4.2 state.json — Real-Time Execution State**

**Location:** %ProgramData%\\EasySave\\state.json

**Updated by:** EasySave after every file transfer

**Format:** JSON, UTF-8, indented

**Write method:** Atomic (written to .tmp then renamed) — never partially written

### **4.3 settings.json — Application Settings and Overrides**

**Location:** %ProgramData%\\EasySave\\settings.json

**Required:** No — application creates a default one if absent.

**Menu Access:** Users can modify key settings interactively via **Option 7** in the EasySave main menu.

{  
  "ConfigFilePath": "",  
  "StateFilePath": "",  
  "LogDirectory": "",  
  "LogFormat": "Xml",  
  "Language": "en"  
}

| Field | Description | Default if empty/invalid |
| :---- | :---- | :---- |
| ConfigFilePath | Override path for config.json | %ProgramData%\\EasySave\\config.json |
| StateFilePath | Override path for state.json | %ProgramData%\\EasySave\\state.json |
| LogDirectory | Override directory for log files | %ProgramData%\\EasySave\\logs\\ |
| LogFormat | Preferred log format ("Json" or "Xml") | "Json" |
| Language | Application language ("en" or "fr") | "en" |

## **5\. Log Files**

### **5.1 Daily Log File**

**Location:** %ProgramData%\\EasySave\\logs\\yyyy-MM-dd.json (or .xml)

**Example:** C:\\ProgramData\\EasySave\\logs\\2026-05-10.xml

**Created by:** EasyLog.dll — one new file per calendar day

**Format:** Entries appended sequentially, UTF-8, formatted based on settings.json.

**Key fields for support diagnosis:**

| Field | Description |
| :---- | :---- |
| transferTimeMs | Transfer time in ms. **Negative value \= copy error** |
| isError | true if the file could not be copied |
| sourceFile | Full UNC path — use to locate the original file |
| destFile | Full UNC path — use to verify the copy destination |

## **6\. Required Permissions**

| Location | Required Permission |
| :---- | :---- |
| %ProgramData%\\EasySave\\ | Read \+ Write (created automatically on first run) |
| %ProgramData%\\EasySave\\logs\\ | Read \+ Write (created automatically) |
| Source directories | Read |
| Target directories | Read \+ Write \+ Create directories |
| Network shares (UNC) | Network access \+ appropriate share permissions |

## **7\. Common Issues and Resolutions**

| Symptom | Likely Cause | Resolution |
| :---- | :---- | :---- |
| Logs outputting in wrong format | LogFormat not correctly set in settings.json | Ask the user to select option 7 in the menu and choose the correct format, or manually edit settings.json |
| "Source or target path not found" | Path does not exist or network share is unreachable | Verify path accessibility from the machine running EasySave |

## **8\. Deployment Checklist**

Before deploying EasySave on a customer server, verify the following:

* \[ \] .NET 8.0 Runtime is installed on the target machine  
* \[ \] EasySave.exe and EasyLog.dll are in the same directory  
* \[ \] %ProgramData%\\EasySave\\settings.json is configured if default paths or XML logs are required  
* \[ \] The application user has **write access** to %ProgramData%\\EasySave\\  
* \[ \] A **maintenance task** is scheduled if CLI mode is used (Windows Task Scheduler)

*ProSoft — Internal support documentation — Do not distribute to end users* *Document version: 1.1 — May 2026*