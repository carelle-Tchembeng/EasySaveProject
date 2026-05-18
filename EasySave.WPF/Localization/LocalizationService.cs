// EasySave.WPF/Localization/LocalizationService.cs
// UPDATED v3.0 — new keys: pause/resume/stop, priority extensions, log storage modes, job editor, settings sections

using System.Collections.Generic;
using System.ComponentModel;

namespace EasySave.WPF.Localization
{
    public class LocalizationService : INotifyPropertyChanged
    {
        private const string French  = "fr";
        private const string English = "en";

        private string _currentLang;
        private readonly Dictionary<string, Dictionary<string, string>> _resources;

        public string this[string key] => Get(key);

        public event EventHandler?              LanguageChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationService()
        {
            _resources   = LoadResources();
            _currentLang = DetectSystemLanguage();
        }

        public string Get(string key)
        {
            if (_resources.TryGetValue(_currentLang, out var t) && t.TryGetValue(key, out var v)) return v;
            if (_resources.TryGetValue(English,       out var e) && e.TryGetValue(key, out var ev)) return ev;
            return key;
        }

        public void SetLanguage(string culture)
        {
            string lang = culture?.ToLower().Substring(0, 2) ?? English;
            if (_currentLang == lang) return;
            _currentLang = _resources.ContainsKey(lang) ? lang : English;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }

        public string CurrentLanguage() => _currentLang;

        private static string DetectSystemLanguage() =>
            System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == French
                ? French : English;

        private static Dictionary<string, Dictionary<string, string>> LoadResources() => new()
        {
            [French] = new Dictionary<string, string>
            {
                // ── App ────────────────────────────────────────────────────
                ["app.title"]              = "EasySave v3.0",
                // ── Job status ─────────────────────────────────────────────
                ["jobs.list.empty"]        = "Aucun travail configuré.",
                ["job.status.inactive"]    = "Inactif",
                ["job.status.active"]      = "En cours",
                ["job.status.paused"]      = "En pause",       // NEW v3.0
                ["job.status.completed"]   = "Terminé",
                ["job.status.error"]       = "Erreur",
                // ── Toolbar buttons ────────────────────────────────────────
                ["btn.run.all"]            = "Exécuter tout",
                ["btn.pause.all"]          = "Tout mettre en pause",   // NEW v3.0
                ["btn.resume.all"]         = "Tout reprendre",         // NEW v3.0
                ["btn.stop.all"]           = "Tout arrêter",           // NEW v3.0
                ["btn.pause"]              = "Pause",                  // NEW v3.0
                ["btn.resume"]             = "Reprendre",              // NEW v3.0
                ["btn.stop"]              = "Arrêter",                // NEW v3.0
                ["btn.add"]               = "Ajouter",
                ["btn.edit"]              = "Modifier",
                ["btn.delete"]            = "Supprimer",
                ["btn.settings"]          = "Paramètres",
                ["btn.save"]              = "Enregistrer",
                ["btn.cancel"]            = "Annuler",
                // ── Settings ───────────────────────────────────────────────
                ["settings.title"]                  = "Paramètres",
                ["settings.language"]               = "Langue :",
                ["settings.log.format"]             = "Format du log :",
                ["settings.business.software"]      = "Logiciel métier :",
                ["settings.crypto.path"]            = "Chemin CryptoSoft :",
                ["settings.extensions"]             = "Extensions à chiffrer :",
                ["settings.priority.extensions"]    = "Extensions prioritaires :",   // NEW v3.0
                ["settings.max.file.size"]          = "Taille max fichier parallèle (Ko) :",  // NEW v3.0
                ["settings.log.storage.mode"]       = "Stockage des logs :",         // NEW v3.0
                ["settings.log.server.url"]         = "URL serveur de logs :",       // NEW v3.0
                // ── Log storage modes ──────────────────────────────────────
                ["log.mode.local"]                  = "Local uniquement",             // NEW v3.0
                ["log.mode.remote"]                 = "Serveur Docker uniquement",    // NEW v3.0
                ["log.mode.both"]                   = "Local + Serveur Docker",       // NEW v3.0
                // ── Columns ────────────────────────────────────────────────
                ["col.name"]              = "Nom",
                ["col.source"]            = "Source",
                ["col.target"]            = "Cible",
                ["col.status"]            = "État",
                ["col.progress"]          = "Progression",
                ["col.actions"]           = "Actions",          // NEW v3.0
                // ── Toolbar — label "Tous les travaux" ────────────────────
                ["toolbar.all.jobs"]        = "Tous les travaux :",
                // ── Job editor dialog ──────────────────────────────────────
                ["job.editor.title"]        = "Éditeur de travail",
                ["job.field.name"]          = "Nom :",
                ["job.field.source"]        = "Chemin source :",
                ["job.field.target"]        = "Chemin cible :",
                ["job.field.type"]          = "Type de sauvegarde :",
                ["job.type.full"]           = "Complète",
                ["job.type.differential"]   = "Différentielle",
                // ── Settings sections ──────────────────────────────────────
                ["settings.section.general"]    = "GÉNÉRAL",
                ["settings.section.security"]   = "SÉCURITÉ",
                ["settings.section.parallel"]   = "TRANSFERTS PARALLÈLES",
                ["settings.section.log.docker"] = "CENTRALISATION DES LOGS (DOCKER)",
                ["settings.hint.parallel"]      = "Les fichiers plus grands que ce seuil utilisent un verrou d'exclusion mutuelle (0 = désactivé).",
                ["settings.hint.log.server"]    = "Exemple : http://logserver:5000 (laisser vide pour désactiver les logs distants)",
                // ── Errors ────────────────────────────────────────────────
                ["error.business.software"] = "Logiciel métier détecté. Sauvegardes mises en pause."
            },
            [English] = new Dictionary<string, string>
            {
                // ── App ────────────────────────────────────────────────────
                ["app.title"]              = "EasySave v3.0",
                // ── Job status ─────────────────────────────────────────────
                ["jobs.list.empty"]        = "No backup jobs configured.",
                ["job.status.inactive"]    = "Inactive",
                ["job.status.active"]      = "Running",
                ["job.status.paused"]      = "Paused",          // NEW v3.0
                ["job.status.completed"]   = "Completed",
                ["job.status.error"]       = "Error",
                // ── Toolbar buttons ────────────────────────────────────────
                ["btn.run.all"]            = "Run All",
                ["btn.pause.all"]          = "Pause All",       // NEW v3.0
                ["btn.resume.all"]         = "Resume All",      // NEW v3.0
                ["btn.stop.all"]           = "Stop All",        // NEW v3.0
                ["btn.pause"]              = "Pause",           // NEW v3.0
                ["btn.resume"]             = "Resume",          // NEW v3.0
                ["btn.stop"]              = "Stop",            // NEW v3.0
                ["btn.add"]               = "Add",
                ["btn.edit"]              = "Edit",
                ["btn.delete"]            = "Delete",
                ["btn.settings"]          = "Settings",
                ["btn.save"]              = "Save",
                ["btn.cancel"]            = "Cancel",
                // ── Settings ───────────────────────────────────────────────
                ["settings.title"]                  = "Settings",
                ["settings.language"]               = "Language:",
                ["settings.log.format"]             = "Log format:",
                ["settings.business.software"]      = "Business software:",
                ["settings.crypto.path"]            = "CryptoSoft path:",
                ["settings.extensions"]             = "Encrypted extensions:",
                ["settings.priority.extensions"]    = "Priority extensions:",     // NEW v3.0
                ["settings.max.file.size"]          = "Max parallel file size (KB):", // NEW v3.0
                ["settings.log.storage.mode"]       = "Log storage:",             // NEW v3.0
                ["settings.log.server.url"]         = "Log server URL:",          // NEW v3.0
                // ── Log storage modes ──────────────────────────────────────
                ["log.mode.local"]                  = "Local only",                // NEW v3.0
                ["log.mode.remote"]                 = "Docker server only",        // NEW v3.0
                ["log.mode.both"]                   = "Local + Docker server",     // NEW v3.0
                // ── Columns ────────────────────────────────────────────────
                ["col.name"]              = "Name",
                ["col.source"]            = "Source",
                ["col.target"]            = "Target",
                ["col.status"]            = "Status",
                ["col.progress"]          = "Progress",
                ["col.actions"]           = "Actions",          // NEW v3.0
                // ── Toolbar — label "All jobs" ─────────────────────────────
                ["toolbar.all.jobs"]        = "All jobs:",
                // ── Job editor dialog ──────────────────────────────────────
                ["job.editor.title"]        = "Backup Job",
                ["job.field.name"]          = "Name:",
                ["job.field.source"]        = "Source path:",
                ["job.field.target"]        = "Target path:",
                ["job.field.type"]          = "Backup type:",
                ["job.type.full"]           = "Full",
                ["job.type.differential"]   = "Differential",
                // ── Settings sections ──────────────────────────────────────
                ["settings.section.general"]    = "GENERAL",
                ["settings.section.security"]   = "SECURITY",
                ["settings.section.parallel"]   = "PARALLEL TRANSFERS",
                ["settings.section.log.docker"] = "LOG CENTRALISATION (DOCKER)",
                ["settings.hint.parallel"]      = "Files larger than this threshold use a mutual-exclusion lock (0 = disabled).",
                ["settings.hint.log.server"]    = "Example: http://logserver:5000  (leave empty to disable remote logging)",
                // ── Errors ────────────────────────────────────────────────
                ["error.business.software"] = "Business software detected. Backups paused."
            }
        };
    }
}
