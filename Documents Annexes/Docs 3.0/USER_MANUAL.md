# EasySave v3.0 — Manuel d'utilisation

**ProSoft** | Version 3.0.0 | Support : contrat de maintenance 5/7 8h-17h

---

## Démarrage rapide

**Lancement GUI :** Double-cliquez sur `EasySave.exe`.  
**Lancement CLI :** `EasySave.exe 1-3` (travaux 1 à 3) ou `EasySave.exe 1;3` (travaux 1 et 3).

---

## Interface principale

La fenêtre affiche la liste des travaux de sauvegarde configurés avec leur statut et progression.

**Barre d'outils (ligne 1) :** `Exécuter tout` · `Ajouter` · `Modifier` · `Supprimer` · `Paramètres`  
**Barre de contrôle (ligne 2 — NOUVEAU v3.0) :** `Tout mettre en pause` · `Tout reprendre` · `Tout arrêter`  
**Colonnes du tableau :** Nom · Source · Cible · État · Progression · Actions (▶ ⏸ ⏵ ⏹ par travail)

**États possibles :** Inactif · En cours · **En pause** (nouveau) · Terminé · Erreur

---

## Créer / modifier un travail

1. Cliquez sur `Ajouter` (ou sélectionnez un travail puis `Modifier`).
2. Renseignez : **Nom**, **Répertoire source**, **Répertoire cible**, **Type** (Complète ou Différentielle).
3. Les chemins peuvent être locaux (`C:\`), disques externes ou lecteurs réseau (UNC `\\serveur\partage`).
4. Cliquez sur `Enregistrer`.

---

## Exécuter des sauvegardes

- **Exécuter tout** : lance tous les travaux **en parallèle** (v3.0).
- **▶ (par travail)** : lance un seul travail.
- Chaque travail affiche sa progression en temps réel.

**Sauvegarde différentielle :** si aucune sauvegarde complète n'a jamais été effectuée pour ce travail, une complète est lancée automatiquement.

---

## Contrôle en temps réel (NOUVEAU v3.0)

| Bouton | Action | Comportement |
|--------|--------|-------------|
| ⏸ Pause | Suspend un travail | Effectif après le fichier en cours |
| ⏵ Reprendre | Reprend depuis l'arrêt | Reprend exactement où le travail s'était arrêté |
| ⏹ Arrêter | Arrêt immédiat | Le fichier en cours est abandonné |
| Tout mettre en pause | Suspend tous les travaux actifs | — |
| Tout reprendre | Reprend tous les travaux pausés | — |
| Tout arrêter | Arrête tous les travaux | — |

**Pause automatique :** si le logiciel métier configuré est détecté, tous les travaux se mettent automatiquement en pause et reprennent dès sa fermeture.

---

## Paramètres

Cliquez sur `Paramètres` pour accéder à :

**GENERAL**  
- *Langue* : `en` ou `fr` (appliqué immédiatement)  
- *Format du log* : `JSON` ou `XML`

**SECURITY**  
- *Chemin CryptoSoft* : chemin vers `CryptoSoft.exe` pour chiffrer les fichiers  
- *Logiciel métier* : nom du processus à surveiller (ex. : `calc`)

**ENCRYPTED EXTENSIONS** — Extensions de fichiers à chiffrer (ex. : `.pdf`, `.docx`)

**PRIORITY EXTENSIONS (NOUVEAU)** — Extensions traitées en priorité (ex. : `.xlsx`). Aucun fichier non-prioritaire ne démarre tant qu'un fichier prioritaire est en attente.

**PARALLEL TRANSFERS (NOUVEAU)** — *Taille max fichier parallèle (Ko)* : seuil au-delà duquel un seul fichier peut être transféré à la fois (0 = désactivé).

**LOG CENTRALISATION (NOUVEAU)** — *Mode* : `Local` / `Remote` / `Both`. *URL serveur* : adresse du serveur Docker (ex. : `http://logserver:5000`).

---

## Fichiers générés

| Fichier | Emplacement | Description |
|---------|-------------|-------------|
| `yyyy-MM-dd.json` (ou `.xml`) | `%ProgramData%\EasySave\logs\` | Log journalier de toutes les actions |
| `state.json` | `%ProgramData%\EasySave\` | État temps réel de chaque travail |
| `config.json` | `%ProgramData%\EasySave\` | Liste des travaux configurés |
| `appconfig.json` | `%ProgramData%\EasySave\` | Paramètres de l'application |
