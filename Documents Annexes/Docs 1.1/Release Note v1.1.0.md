# **Release Note — EasySave v1.1.0**

**ProSoft** | Release date: May 2026

## **Overview**

This is the **version 1.1** update of EasySave. This release focuses on expanding logging capabilities, improving configuration management, and providing a better interactive experience directly from the console interface.

## **What's New in v1.1.0**

### **Interface & Settings Management**

* **General Settings Menu** — The main interactive menu has been expanded to 8 options. Option 7\. General settings has been introduced, allowing users to configure application-wide preferences directly from the console without manually modifying configuration files.  
* **Language Persistence** — The preferred interface language (Language) is now saved reliably in settings.json, removing the need for auto-detection on every startup if already configured.

### **Logging Enhancements**

* **XML Log Format Support** — Users can now choose to generate daily log files in either JSON (default) or XML format. This ensures better compatibility with legacy systems or specific IT compliance requirements.  
* **Dynamic Format Toggling** — Through the new Settings menu (Option 7), users can instantly toggle the current log format (\[1=JSON / 2=XML\]). The change is applied immediately for all subsequent backup jobs.  
* **Upgraded Library** — The log format logic is fully integrated into the updated EasyLog.dll library (v1.1.0).

### **Configuration Improvements**

* **Centralized Settings in ProgramData** — The application settings file (settings.json) is no longer stored next to the executable. It has been relocated to %ProgramData%\\EasySave\\settings.json. This prevents permission issues, avoids hardcoded temporary paths, and makes EasySave fully suitable for multi-user client/server environments.

## **Technical Specifications**

| Property | Value |
| :---- | :---- |
| Target framework | .NET 8.0 |
| Language | C\# |
| Architecture | 4-layer — Core / Infrastructure / EasyLog / ConsoleApp |
| Design patterns | Strategy, Repository, Adapter, Singleton, Dependency Injection |
| Build tool | Visual Studio 2022 |
| Version control | GitHub |

## **Known Limitations**

| Limitation | Detail |
| :---- | :---- |
| Maximum backup jobs | 5 jobs per installation |
| Execution mode | Sequential only — no parallel execution in v1.1 |
| Platform | Windows only — Linux/macOS not supported |
| Interface | Console only — no graphical interface in v1.1 |
| Log rotation | No built-in log rotation — manage log files at OS level |

## **Upgrade Notes**

To upgrade from v1.0.0 to v1.1.0:

1. Replace EasySave.exe and EasyLog.dll in the installation directory.  
2. If you previously used an appsettings.json file in the installation directory, move its contents to the new %ProgramData%\\EasySave\\settings.json file.  
3. Your existing backup jobs (config.json) and live states (state.json) are fully compatible and will be preserved automatically.

## **Compatibility**

| Component | Compatibility |
| :---- | :---- |
| EasyLog.dll | v1.1.0 — Backward compatible with v1.0.0 implementations |
| config.json | Fully compatible with v1.0.0 |
| state.json | Fully compatible with v1.0.0 |

## **Roadmap — Planned for v2.0**

The following features are planned for the next major version:

* Graphical user interface (WPF) based on the **MVVM architecture**  
* Parallel execution of backup jobs  
* The Core layer (business logic) will remain strictly isolated and reused without modification

*EasySave v1.1.0 — ProSoft © 2024-2026. All rights reserved.* *Maintenance contract: 5/7, 08:00–17:00 — Annual renewal with SYNTEC index revaluation.*