// EasySave.WPF/Localization/LocalizationService.cs
// WPF equivalent of ConsoleApp ResourceLocalizer with LanguageChanged event

namespace EasySave.WPF.Localization
{
    /// <summary>
    /// FR/EN in-memory localization service for WPF ViewModels.
    /// Exposes LanguageChanged event so ViewModels can refresh bindings on language switch.
    /// </summary>
    public class LocalizationService
    {
        private const string French  = "fr";
        private const string English = "en";

        private string _currentLang;
        private readonly Dictionary<string, Dictionary<string, string>> _resources;
        public string this[string key] => Get(key);

        public event EventHandler? LanguageChanged;

        public LocalizationService()
        {
            _resources   = LoadResources();
            _currentLang = DetectSystemLanguage();
        }

        public string Get(string key)
        {
            if (_resources.TryGetValue(_currentLang, out var t) && t.TryGetValue(key, out var v)) return v;
            if (_resources.TryGetValue(English, out var e) && e.TryGetValue(key, out var ev)) return ev;
            return key;
        }

        public void SetLanguage(string culture)
        {
            string lang = culture?.ToLower()[..2] ?? English;
            _currentLang = _resources.ContainsKey(lang) ? lang : English;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string CurrentLanguage() => _currentLang;

        private static string DetectSystemLanguage() =>
            System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == French
                ? French : English;

        private static Dictionary<string, Dictionary<string, string>> LoadResources() => new()
        {
            [French] = new Dictionary<string, string>
            {
                ["app.title"]                  = "EasySave v2.0",
                ["jobs.list.empty"]            = "Aucun travail configuré.",
                ["job.status.inactive"]        = "Inactif",
                ["job.status.active"]          = "En cours",
                ["job.status.completed"]       = "Terminé",
                ["job.status.error"]           = "Erreur",
                ["btn.run.all"]                = "Exécuter tout",
                ["btn.add"]                    = "Ajouter",
                ["btn.edit"]                   = "Modifier",
                ["btn.delete"]                 = "Supprimer",
                ["btn.settings"]               = "Paramètres",
                ["btn.save"]                   = "Enregistrer",
                ["btn.cancel"]                 = "Annuler",
                ["settings.title"]             = "Paramètres",
                ["settings.log.format"]        = "Format du log :",
                ["settings.business.software"] = "Logiciel métier :",
                ["settings.crypto.path"]       = "Chemin CryptoSoft :",
                ["settings.extensions"]        = "Extensions à chiffrer :",
                ["settings.language"]          = "Langue :",
                ["error.business.software"]    = "Logiciel métier détecté. Sauvegarde bloquée.",
            },
            [English] = new Dictionary<string, string>
            {
                ["app.title"]                  = "EasySave v2.0",
                ["jobs.list.empty"]            = "No backup jobs configured.",
                ["job.status.inactive"]        = "Inactive",
                ["job.status.active"]          = "Running",
                ["job.status.completed"]       = "Completed",
                ["job.status.error"]           = "Error",
                ["btn.run.all"]                = "Run All",
                ["btn.add"]                    = "Add",
                ["btn.edit"]                   = "Edit",
                ["btn.delete"]                 = "Delete",
                ["btn.settings"]               = "Settings",
                ["btn.save"]                   = "Save",
                ["btn.cancel"]                 = "Cancel",
                ["settings.title"]             = "Settings",
                ["settings.log.format"]        = "Log format:",
                ["settings.business.software"] = "Business software:",
                ["settings.crypto.path"]       = "CryptoSoft path:",
                ["settings.extensions"]        = "Encrypted extensions:",
                ["settings.language"]          = "Language:",
                ["error.business.software"]    = "Business software detected. Backup blocked.",
            }
        };
    }
}
