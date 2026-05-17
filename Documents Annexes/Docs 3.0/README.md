# EasySave v3.0

**Logiciel de sauvegarde professionnel — ProSoft**

[![Version](https://img.shields.io/badge/version-3.0.0-green)](.)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)](.)
[![Docker](https://img.shields.io/badge/LogServer-Docker-2496ED)](.)

---

## Description

EasySave est un logiciel de sauvegarde professionnel développé par ProSoft. La version 3.0 introduit l'exécution **parallèle** des sauvegardes, le contrôle **temps réel** (pause/reprise/arrêt), la gestion des **fichiers prioritaires**, le chiffrement **mono-instance** via CryptoSoft, et la **centralisation des logs** sur un serveur Docker partagé.

---

## Structure du projet

```
EasySaveProject/
├── EasyLog/                    ← DLL de logging (JSON/XML) — partagée entre versions
├── EasySave.Core/              ← Logique métier (aucune dépendance externe)
├── EasySave.Infrastructure/    ← Implémentations concrètes (filesystem, logs, encryption)
├── EasySave.WPF/               ← Interface graphique WPF (MVVM)
├── EasySave.LogServer/         ← Service Docker de centralisation des logs (NEW v3.0)
├── EasySave.sln                ← Solution Visual Studio
├── docker-compose.yml          ← Déploiement Docker du LogServer
└── Documents Annexes/          ← Diagrammes UML, cahiers des charges, documentation
```

---

## Prérequis

- Visual Studio 2022 ou supérieur
- .NET 8.0 SDK
- Docker Desktop 24+ (optionnel, pour le LogServer)
- Git

---

## Compilation

```bash
# Cloner le dépôt
git clone <url-repo>
cd EasySaveProject

# Compiler toute la solution
dotnet build EasySave.sln

# Lancer l'application WPF
dotnet run --project EasySave.WPF

# Lancer le LogServer Docker
docker-compose up -d
```

---

## Fonctionnalités v3.0

| Fonctionnalité | Description |
|----------------|-------------|
| **Sauvegarde parallèle** | Tous les travaux s'exécutent simultanément (`Task.WhenAll`) |
| **Fichiers prioritaires** | Extensions configurables — bloquent les non-prioritaires pendant leur transfert |
| **Verrou fichiers volumineux** | Seuil configurable (Ko) — un seul gros fichier à la fois entre tous les travaux |
| **Pause / Reprise / Arrêt** | Par travail individuel ET global — pause après fichier courant, arrêt immédiat |
| **Surveillance logiciel métier** | Auto-pause à la détection, auto-reprise à la fermeture (contrairement à v2.0 qui bloquait) |
| **CryptoSoft mono-instance** | Mutex système nommé — garantit qu'une seule instance CryptoSoft tourne à la fois |
| **LogServer Docker** | Centralisation des logs multi-machines — `Local` / `Remote` / `Both` |
| **Multi-langues** | Français et anglais (clés v3.0 incluses) |
| **CLI compatible** | Syntaxe identique v1.0/1.1/2.0 |

---

## Architecture (principes)

EasySave respecte une architecture en couches stricte :

```
EasySave.WPF (View + ViewModel)
        ↓
EasySave.Core (Entities, Interfaces, Services)  ← aucune dépendance externe
        ↓
EasySave.Infrastructure (Implémentations concrètes)
        ↓
EasyLog.dll (logging, DLL partagée entre versions)
```

**Patterns utilisés :** MVVM · Strategy · Adapter · Repository · Dependency Injection (maison) · Observer (events)

**Synchronisation v3.0 :**
- `PriorityManager` — compteur atomique + `ManualResetEventSlim` pour la priorité inter-threads
- `LargeFileTransferLock` — `SemaphoreSlim(1,1)` pour la sérialisation des gros fichiers
- `JobExecutionContext` — `ManualResetEventSlim` (pause) + `CancellationTokenSource` (stop) par travail

---

## Gestion des versions

| Version | Branche Git | Description |
|---------|------------|-------------|
| 1.0 | `version-1.0` | Console — 5 travaux max — logs JSON |
| 1.1 | `version-1.1` | Console — logs JSON/XML |
| 2.0 | `version-2.0` | WPF — travaux illimités — CryptoSoft — logiciel métier |
| **3.0** | **`version-3.0`** | **WPF — parallèle — priorité — pause/reprise — Docker** |

---

## Conventions de code

- **Langue :** Anglais (code, commentaires, noms de méthodes/classes)
- **Nommage :** PascalCase (types/méthodes), camelCase (variables), `_camelCase` (champs privés)
- **Taille des fonctions :** < 40 lignes recommandées
- **Pas de duplication :** code partagé via interfaces et classes de base
- **Nullable** : activé — toutes les nullabilités sont explicites

---

## Équipe et contacts

Projet développé dans le cadre du fil rouge **EasySave — ProSoft**.  
Supervisé par le DSI de ProSoft.

**Support client :** Contrat de maintenance 5/7 8h-17h | 12% du prix d'achat | Revalorisation indice SYNTEC.
