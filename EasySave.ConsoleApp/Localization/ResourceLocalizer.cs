// EasySave.ConsoleApp/Localization/ResourceLocalizer.cs

namespace EasySave.ConsoleApp.Localization
{
    /// <summary>
    /// In-memory implementation of ILocalizer.
    /// Translations are stored in a nested dictionary:
    ///   language code → translation key → translated string.
    /// Detects the system language automatically at startup.
    /// Falls back to English for any unsupported language.
    /// </summary>
    public class ResourceLocalizer : ILocalizer
    {
        // ─────────────────────────────────────────────────────────────
        // Supported language codes
        // ─────────────────────────────────────────────────────────────

        private const string French  = "fr";
        private const string English = "en";

        // ─────────────────────────────────────────────────────────────
        // State
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Currently active language code. Defaults to system language.
        /// </summary>
        private string _currentLang;

        /// <summary>
        /// All available translations.
        /// Structure: language → key → value
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> _resources;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the localizer, loads all translations,
        /// and detects the system language.
        /// </summary>
        public ResourceLocalizer()
        {
            _resources   = LoadResources();
            _currentLang = DetectSystemLanguage();
        }

        // ─────────────────────────────────────────────────────────────
        // ILocalizer implementation
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public string Get(string key)
        {
            if (_resources.TryGetValue(_currentLang, out var translations) &&
                translations.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to English if key not found in current language
            if (_resources.TryGetValue(English, out var englishTranslations) &&
                englishTranslations.TryGetValue(key, out var englishValue))
            {
                return englishValue;
            }

            // Return the key itself if no translation found (never crash)
            return key;
        }

        /// <inheritdoc/>
        public void SetLanguage(string culture)
        {
            string lang = culture?.ToLower()?.Substring(0, 2) ?? English;
            _currentLang = _resources.ContainsKey(lang) ? lang : English;
        }

        /// <inheritdoc/>
        public string CurrentLanguage() => _currentLang;

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects the operating system UI language.
        /// Returns "fr" for French, "en" for everything else.
        /// </summary>
        private static string DetectSystemLanguage()
        {
            string culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return culture == French ? French : English;
        }

        /// <summary>
        /// Builds and returns the complete translation dictionary.
        /// Add new keys here for both languages simultaneously.
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> LoadResources()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                [French] = new Dictionary<string, string>
                {
                    // ── Application ──────────────────────────────────
                    ["app.title"]              = "=== EasySave v1.0 ===",
                    ["app.language.prompt"]    = "Sélectionnez votre langue / Select your language [fr/en] :",
                    ["app.exit"]               = "Au revoir !",

                    // ── Main menu ────────────────────────────────────
                    ["menu.title"]             = "--- MENU PRINCIPAL ---",
                    ["menu.list"]              = "1. Lister les travaux de sauvegarde",
                    ["menu.create"]            = "2. Créer un travail de sauvegarde",
                    ["menu.edit"]              = "3. Modifier un travail de sauvegarde",
                    ["menu.delete"]            = "4. Supprimer un travail de sauvegarde",
                    ["menu.execute.one"]       = "5. Exécuter un travail de sauvegarde",
                    ["menu.execute.all"]       = "6. Exécuter tous les travaux",
                    ["menu.quit"]              = "7. Quitter",
                    ["menu.choice"]            = "Votre choix :",
                    ["menu.invalid"]           = "Choix invalide. Veuillez réessayer.",

                    // ── Job list ─────────────────────────────────────
                    ["job.list.title"]         = "--- TRAVAUX DE SAUVEGARDE ---",
                    ["job.list.empty"]         = "Aucun travail de sauvegarde configuré.",
                    ["job.list.header"]        = "  ID | Nom              | Type          | Source → Cible",

                    // ── Job form ─────────────────────────────────────
                    ["job.form.name"]          = "Nom du travail :",
                    ["job.form.source"]        = "Répertoire source :",
                    ["job.form.target"]        = "Répertoire cible :",
                    ["job.form.type"]          = "Type [1=Complet / 2=Différentiel] :",
                    ["job.form.type.invalid"]  = "Type invalide. Entrez 1 ou 2.",
                    ["job.select.index"]       = "Numéro du travail :",
                    ["job.confirm.delete"]     = "Confirmer la suppression de '{0}' ? [o/n] :",

                    // ── Execution ────────────────────────────────────
                    ["exec.starting"]          = "Démarrage de la sauvegarde : {0}",
                    ["exec.progress"]          = "  [{0}%] {1}/{2} fichiers — {3} restants",
                    ["exec.done"]              = "Sauvegarde terminée : {0}",
                    ["exec.error"]             = "Erreur lors de la sauvegarde : {0}",
                    ["exec.all.starting"]      = "Exécution de tous les travaux ({0} au total)...",
                    ["exec.all.done"]          = "Tous les travaux sont terminés.",

                    // ── Success / Error messages ──────────────────────
                    ["success.job.created"]    = "Travail créé avec succès.",
                    ["success.job.updated"]    = "Travail mis à jour avec succès.",
                    ["success.job.deleted"]    = "Travail supprimé avec succès.",
                    ["error.max.jobs"]         = "Limite atteinte : 5 travaux maximum.",
                    ["error.path.invalid"]     = "Chemin source ou cible introuvable.",
                    ["error.index.invalid"]    = "Numéro de travail invalide.",
                    ["error.empty.input"]      = "Ce champ ne peut pas être vide.",
                    ["error.no.jobs"]          = "Aucun travail configuré à exécuter.",
                    ["error.delete.cancelled"] = "Suppression annulée.",

                    // ── Prompts ──────────────────────────────────────
                    ["prompt.press.enter"]     = "Appuyez sur Entrée pour continuer..."
                },

                [English] = new Dictionary<string, string>
                {
                    // ── Application ──────────────────────────────────
                    ["app.title"]              = "=== EasySave v1.0 ===",
                    ["app.language.prompt"]    = "Sélectionnez votre langue / Select your language [fr/en] :",
                    ["app.exit"]               = "Goodbye!",

                    // ── Main menu ────────────────────────────────────
                    ["menu.title"]             = "--- MAIN MENU ---",
                    ["menu.list"]              = "1. List backup jobs",
                    ["menu.create"]            = "2. Create a backup job",
                    ["menu.edit"]              = "3. Edit a backup job",
                    ["menu.delete"]            = "4. Delete a backup job",
                    ["menu.execute.one"]       = "5. Execute a backup job",
                    ["menu.execute.all"]       = "6. Execute all backup jobs",
                    ["menu.quit"]              = "7. Quit",
                    ["menu.choice"]            = "Your choice:",
                    ["menu.invalid"]           = "Invalid choice. Please try again.",

                    // ── Job list ─────────────────────────────────────
                    ["job.list.title"]         = "--- BACKUP JOBS ---",
                    ["job.list.empty"]         = "No backup jobs configured.",
                    ["job.list.header"]        = "  ID | Name             | Type          | Source → Target",

                    // ── Job form ─────────────────────────────────────
                    ["job.form.name"]          = "Job name:",
                    ["job.form.source"]        = "Source directory:",
                    ["job.form.target"]        = "Target directory:",
                    ["job.form.type"]          = "Type [1=Full / 2=Differential]:",
                    ["job.form.type.invalid"]  = "Invalid type. Enter 1 or 2.",
                    ["job.select.index"]       = "Job number:",
                    ["job.confirm.delete"]     = "Confirm deletion of '{0}'? [y/n]:",

                    // ── Execution ────────────────────────────────────
                    ["exec.starting"]          = "Starting backup: {0}",
                    ["exec.progress"]          = "  [{0}%] {1}/{2} files — {3} remaining",
                    ["exec.done"]              = "Backup completed: {0}",
                    ["exec.error"]             = "Backup error: {0}",
                    ["exec.all.starting"]      = "Running all jobs ({0} total)...",
                    ["exec.all.done"]          = "All backup jobs completed.",

                    // ── Success / Error messages ──────────────────────
                    ["success.job.created"]    = "Job created successfully.",
                    ["success.job.updated"]    = "Job updated successfully.",
                    ["success.job.deleted"]    = "Job deleted successfully.",
                    ["error.max.jobs"]         = "Limit reached: maximum 5 jobs allowed.",
                    ["error.path.invalid"]     = "Source or target path not found.",
                    ["error.index.invalid"]    = "Invalid job number.",
                    ["error.empty.input"]      = "This field cannot be empty.",
                    ["error.no.jobs"]          = "No jobs configured to execute.",
                    ["error.delete.cancelled"] = "Deletion cancelled.",

                    // ── Prompts ──────────────────────────────────────
                    ["prompt.press.enter"]     = "Press Enter to continue..."
                }
            };
        }
    }
}
