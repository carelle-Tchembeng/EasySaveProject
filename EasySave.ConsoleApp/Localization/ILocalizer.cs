// EasySave.ConsoleApp/Localization/ILocalizer.cs

namespace EasySave.ConsoleApp.Localization
{
    /// <summary>
    /// Defines the contract for string localization.
    /// Provides translated strings for all user-facing messages
    /// based on the currently active language.
    /// Supported languages: French (fr), English (en).
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// Returns the translated string for the specified key
        /// in the currently active language.
        /// If the key is not found, returns the key itself
        /// to avoid crashes on missing translations.
        /// </summary>
        /// <param name="key">Translation key. Example: "menu.title"</param>
        /// <returns>Translated string, or the key if not found.</returns>
        string Get(string key);

        /// <summary>
        /// Sets the active language for all subsequent Get() calls.
        /// </summary>
        /// <param name="culture">
        /// Language code. Supported values: "fr", "en".
        /// Falls back to "en" for any unsupported value.
        /// </param>
        void SetLanguage(string culture);

        /// <summary>
        /// Returns the currently active language code.
        /// Example: "fr" or "en"
        /// </summary>
        string CurrentLanguage();
    }
}
