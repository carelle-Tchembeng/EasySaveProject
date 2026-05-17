# Release Note — EasySave v3.0.0

**Éditeur :** ProSoft  
**Date de publication :** 2024  
**Version :** 3.0.0  
**Versions compatibles :** Mise à jour depuis v2.0

---

## Résumé des évolutions

EasySave v3.0 est une mise à jour majeure qui transforme l'exécution séquentielle des sauvegardes en exécution **parallèle**, ajoute une gestion fine du **contrôle en temps réel** (pause/reprise/arrêt par travail et global), introduit des mécanismes de **priorité fichiers** et de **protection de bande passante**, rend CryptoSoft **mono-instance**, et ajoute un service de **centralisation des logs** déployable sous Docker.

---

## Nouvelles fonctionnalités

### 1. Sauvegarde en parallèle
Les travaux de sauvegarde s'exécutent désormais **simultanément** via `Task.WhenAll()`. L'exécution séquentielle est abandonnée en faveur du parallélisme natif .NET.  
**Impact :** Réduction significative du temps total de sauvegarde sur les environnements multi-jobs.

### 2. Gestion des fichiers prioritaires
Une liste d'extensions prioritaires est configurable dans les paramètres généraux (ex. : `.xlsx`, `.docx`).  
**Règle :** Aucun fichier non-prioritaire ne peut être transféré tant qu'il reste au moins un fichier prioritaire en attente sur n'importe quel travail actif.  
**Implémentation :** `PriorityManager` (compteur atomique + `ManualResetEventSlim`).

### 3. Verrou transfert fichiers volumineux
Un seuil de taille configurable (en Ko) empêche deux fichiers dépassant ce seuil d'être transférés simultanément.  
**Règle :** Pendant le transfert d'un fichier volumineux, les autres travaux peuvent transférer des fichiers plus petits (sous réserve de la règle priorité).  
**Implémentation :** `LargeFileTransferLock` (`SemaphoreSlim(1,1)`).

### 4. Contrôle en temps réel — Pause / Reprise / Arrêt
Chaque travail de sauvegarde, ainsi que l'ensemble des travaux, peuvent être contrôlés via :
- **Pause** : la pause prend effet après le fichier en cours de transfert (cohérence garantie).
- **Reprise** : les travaux reprennent exactement là où ils s'étaient arrêtés.
- **Arrêt** : arrêt immédiat, le fichier en cours est abandonné.  
**Interface :** Boutons ▶ ⏸ ⏵ ⏹ par ligne dans la DataGrid + boutons globaux dans la toolbar.

### 5. Pause automatique sur détection du logiciel métier
Contrairement à v2.0 (blocage définitif avec statut Erreur), v3.0 **met en pause automatiquement** tous les travaux dès la détection du logiciel métier.  
Les sauvegardes **reprennent automatiquement** à l'arrêt du logiciel métier.  
**Implémentation :** Thread de surveillance dédié, polling toutes les 500 ms.

### 6. CryptoSoft Mono-Instance
CryptoSoft ne peut plus être lancé simultanément par plusieurs threads.  
**Implémentation :** Mutex nommé système `Global\CryptoSoft_EasySave` — garantit l'exclusivité même en cas de parallélisme multi-jobs.

### 7. Centralisation des logs sous Docker
Nouveau projet `EasySave.LogServer` : service ASP.NET Core Minimal API déployable sous Docker.  
**Modes de stockage des logs :**
- `Local` : fichiers journaliers uniquement sur le poste client (comportement identique v2.0).
- `Remote` : uniquement sur le serveur Docker centralisé.
- `Both` : local + serveur Docker simultanément.  
**Fichier unique journalier** : quel que soit le nombre de clients, un seul fichier `yyyy-MM-dd.json` est maintenu sur le serveur.  
**Identification multi-machine** : chaque entrée contient `MachineName` et `ApplicationVersion`.

---

## Modifications techniques

| Composant | Changement |
|-----------|-----------|
| `BackupService` | Passe de `ExecuteAll()` synchrone à `ExecuteAllAsync()` parallèle (`Task.WhenAll`) |
| `IBackupStrategy.Execute()` | 3 nouveaux paramètres : `JobExecutionContext`, `PriorityManager`, `LargeFileTransferLock` |
| `BackupStatus` | Nouveau statut `Paused` (index 2) |
| `AppConfiguration` | 4 nouvelles propriétés : `PriorityExtensions`, `MaxParallelFileSizeKb`, `LogStorageMode`, `LogServerUrl` |
| `CryptoSoftAdapter` | Mutex nommé système pour mono-instance |
| `EasyLogAdapter` | Routage local/remote/both via `ConfigureRemoteLogging()` |
| `MainViewModel` | Ajout `PauseAllCommand`, `ResumeAllCommand`, `StopAllCommand` |
| `BackupJobViewModel` | Ajout `PauseCommand`, `ResumeCommand`, `StopCommand` + progress temps réel |
| `SettingsViewModel` | Ajout gestion extensions prioritaires, seuil fichier, mode log, URL serveur |

---

## Compatibilité

- **CLI** : Syntaxe de ligne de commande identique aux versions 1.0, 1.1, 2.0 (`EasySave.exe 1-3`, `EasySave.exe 1;3`).
- **config.json** : Compatible — les nouveaux champs `AppConfiguration` ont des valeurs par défaut.
- **EasyLog.dll** : Compatible v1.0 — aucune modification de l'interface publique.
- **Log journalier** : Format JSON/XML inchangé. Nouvelle entrée `MachineName` uniquement dans les logs distants.

---

## Configuration requise

| Élément | Requis |
|---------|--------|
| OS | Windows 10 / 11 / Windows Server 2019+ |
| .NET Runtime | .NET 8.0 (Desktop Runtime) |
| Docker (optionnel) | Docker Desktop 24+ ou Docker Engine 24+ |
| CryptoSoft (optionnel) | Compatible v2.0 — aucune modification requise côté client |

---

## Notes de déploiement

**LogServer Docker :**
```bash
# Depuis le dossier EasySaveProject/
docker-compose up -d

# Vérification
curl http://localhost:5000/
```

**Configuration client (appconfig.json) :**
```json
{
  "LogStorageMode": "Both",
  "LogServerUrl": "http://localhost:5000"
}
```

---

## Bugs corrigés

- Correction du comportement v2.0 : la détection du logiciel métier génère désormais une **pause** et non une **erreur** irréversible.
- Le `ProgressPercent` dans la DataGrid est maintenant correctement rafraîchi en temps réel pendant l'exécution parallèle.

---

*ProSoft — Tous droits réservés. Support : contrat de maintenance 5/7 8h-17h.*
