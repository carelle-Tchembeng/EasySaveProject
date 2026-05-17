# EasySave v3.0 — Documentation Support Technique

**ProSoft** | Usage interne support client | Version 3.0.0

---

## 1. Informations générales

| Élément | Valeur |
|---------|--------|
| Nom du logiciel | EasySave |
| Version | 3.0.0 |
| Éditeur | ProSoft |
| Langage / Framework | C# / .NET 8.0 |
| Interface | WPF (Windows Presentation Foundation) |
| Prix unitaire | 200 € HT |
| Maintenance | 12% du prix d'achat / an (contrat 5/7 8h-17h, revalorisation SYNTEC) |

---

## 2. Configuration minimale requise

| Composant | Minimum |
|-----------|---------|
| Système d'exploitation | Windows 10 (64-bit) ou Windows Server 2019 |
| .NET Runtime | .NET 8.0 Desktop Runtime |
| RAM | 512 Mo |
| Espace disque | 50 Mo (logiciel) + espace pour logs |
| Réseau | Recommandé pour lecteurs réseau et/ou LogServer Docker |
| Docker | Docker Desktop 24+ (optionnel, pour LogServer uniquement) |

---

## 3. Emplacements des fichiers

### 3.1 Exécutable
```
%ProgramFiles%\EasySave\EasySave.exe
```
Ou dossier personnalisé selon déploiement client.

### 3.2 Fichiers de configuration
```
C:\ProgramData\EasySave\appconfig.json   ← Paramètres utilisateur (CryptoSoft, logiciel métier, extensions, LogServer...)
C:\ProgramData\EasySave\config.json      ← Liste des travaux de sauvegarde
```

### 3.3 Fichier d'état temps réel
```
C:\ProgramData\EasySave\state.json       ← État de chaque travail (mis à jour en temps réel)
```

### 3.4 Logs journaliers (locaux)
```
C:\ProgramData\EasySave\logs\yyyy-MM-dd.json   ← Format JSON (défaut)
C:\ProgramData\EasySave\logs\yyyy-MM-dd.xml    ← Format XML (si configuré)
```
Chemin personnalisable via `appsettings.json` (clé `LogDirectory`).

### 3.5 Paramètres d'infrastructure
```
[dossier EasySave.exe]\appsettings.json  ← Chemins des fichiers (LogDirectory, ConfigFilePath, StateFilePath, AppConfigFilePath)
```

### 3.6 LogServer Docker (optionnel)
```
Volume Docker : /logs/yyyy-MM-dd.json    ← Fichier journalier centralisé unique (toutes machines confondues)
Endpoint : http://<hôte>:5000/api/log   ← Réception des entrées
Endpoint : http://<hôte>:5000/api/log/status ← Statut et taille du fichier en cours
```

---

## 4. Architecture des projets

```
EasySaveProject/
├── EasyLog/                   ← DLL de log (IEasyLogWriter, JSON/XML formatters) — v1.0 compatible
├── EasySave.Core/             ← Logique métier pure, pas de dépendance externe
│   ├── Entities/              ← BackupJob, AppConfiguration
│   ├── Enums/                 ← BackupStatus (Inactive/Active/Paused/Completed/Error), BackupType
│   ├── Interfaces/            ← IBackupStrategy, IFileSystem, ILogger, IEncryptionService, ...
│   ├── Services/              ← BackupService, JobManager, PriorityManager*, LargeFileTransferLock*, JobExecutionContext*
│   └── ValueObjects/          ← LogEntry, BackupProgress
├── EasySave.Infrastructure/   ← Implémentations : filesystem, repos, strategies, encryption, logging
│   ├── Strategies/            ← FullBackupStrategy, DifferentialBackupStrategy (v3: priority+lock+pause)
│   ├── Encryption/            ← CryptoSoftAdapter (v3: Mutex mono-instance)
│   ├── Logging/               ← EasyLogAdapter (v3: routage local/remote/both)
│   ├── Remote/                ← RemoteLogSender* (HTTP POST vers LogServer Docker)
│   └── Repositories/         ← JsonConfigRepository, JsonStateRepository, JsonAppConfigRepository
├── EasySave.WPF/              ← Interface WPF (MVVM)
│   ├── ViewModels/            ← MainViewModel (v3: PauseAll/ResumeAll/StopAll), BackupJobViewModel (v3: contrôles par job)
│   ├── Views/                 ← MainWindow, SettingsWindow, JobEditorWindow
│   └── Localization/          ← LocalizationService (fr/en, y compris nouvelles clés v3)
└── EasySave.LogServer/        ← NEW v3.0 — Service ASP.NET Core Docker
    ├── Controllers/           ← LogController (POST /api/log, GET /api/log/status)
    ├── Services/              ← ILogStorageService, JsonLogStorageService (thread-safe)
    ├── DTOs/                  ← LogEntryRemoteDto (+ MachineName, ApplicationVersion)
    └── Dockerfile / docker-compose.yml
```
*Nouveaux en v3.0*

---

## 5. Structure de appconfig.json (v3.0)

```json
{
  "cryptoSoftPath": "C:\\Tools\\CryptoSoft.exe",
  "businessSoftwareName": "calc",
  "encryptedExtensions": [".pdf", ".docx"],
  "logFormat": "JSON",
  "defaultLanguage": "fr",
  "priorityExtensions": [".xlsx", ".docx"],
  "maxParallelFileSizeKb": 1024,
  "logStorageMode": "Both",
  "logServerUrl": "http://logserver:5000"
}
```

**Valeurs LogStorageMode :** `"Local"` | `"Remote"` | `"Both"`  
**maxParallelFileSizeKb = 0** : verrou désactivé (aucune restriction sur les fichiers volumineux).

---

## 6. Diagnostic et résolution de problèmes fréquents

### La sauvegarde ne démarre pas
- Vérifier que les chemins source et cible existent et sont accessibles.
- Vérifier que le logiciel métier configuré (`businessSoftwareName`) n'est **pas** en cours d'exécution.
- Consulter le fichier log journalier pour les entrées `SYSTEM` (pause/arrêt automatique).

### La progression ne s'affiche pas en temps réel
- Vérifier que les fichiers à sauvegarder ne sont pas trop petits (la progression peut sembler instantanée).
- Les travaux se mettent à jour toutes les 200 ms dans l'interface.

### Les logs ne remontent pas au serveur Docker
1. Vérifier que le conteneur est démarré : `docker ps | grep easysave-logserver`.
2. Tester l'endpoint : `curl http://localhost:5000/`.
3. Vérifier `LogServerUrl` dans `appconfig.json` (doit être accessible depuis le poste client).
4. Vérifier que `LogStorageMode` est `"Remote"` ou `"Both"`.

### CryptoSoft ne chiffre pas les fichiers
- Vérifier que `CryptoSoftPath` pointe vers un exécutable valide.
- Vérifier que les extensions concernées sont dans `encryptedExtensions`.
- CryptoSoft est mono-instance en v3.0 : si le processus précédent est bloqué, tuer `CryptoSoft.exe` depuis le gestionnaire des tâches.

### Statut "En pause" inattendu
- Le logiciel métier (`businessSoftwareName`) a été détecté. Fermer le logiciel métier pour reprendre automatiquement.
- Si intentionnel : cliquer sur `Tout reprendre` ou `⏵` sur le travail concerné.

---

## 7. Modes CLI (compatibilité v1.0)

```
EasySave.exe 2        → Exécute le travail n°2
EasySave.exe 1-3      → Exécute les travaux 1, 2 et 3 en séquence
EasySave.exe 1;3      → Exécute les travaux 1 et 3 en séquence
```
*Remarque : en CLI, l'exécution reste séquentielle par souci de compatibilité. Le parallélisme est réservé au mode GUI (`Exécuter tout`).*

---

## 8. Déploiement LogServer Docker

```bash
# Lancement
cd EasySaveProject/
docker-compose up -d

# Arrêt
docker-compose down

# Voir les logs du serveur
docker logs easysave-logserver

# Accéder au volume de logs
docker exec easysave-logserver ls /logs/
```

Variables d'environnement du conteneur :
- `LOG_DIR` : répertoire de stockage des logs (défaut `/logs`).
- `ASPNETCORE_URLS` : URL d'écoute (défaut `http://+:5000`).

---

## 9. Contrat de maintenance

- Disponibilité : 5 jours / 7, 8h00 – 17h00
- Inclus : mises à jour mineures, correctifs, support téléphonique et email
- Revalorisation annuelle : indice SYNTEC
- Reconduction tacite
