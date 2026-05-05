// EasySave.ConsoleApp/Parsers/CliArgumentParser.cs

namespace EasySave.ConsoleApp.Parsers
{
    /// <summary>
    /// Parses command-line arguments into a list of 1-based job indices.
    /// Supports two formats:
    ///   Range:  "1-3"  → [1, 2, 3]
    ///   List:   "1;3"  → [1, 3]
    ///
    /// Usage:
    ///   EasySave.exe 1-3   → executes jobs 1, 2 and 3
    ///   EasySave.exe 1;3   → executes jobs 1 and 3
    ///
    /// Invalid indices (out of range or non-numeric) are silently ignored.
    /// An empty result list means no valid indices were found.
    /// </summary>
    public class CliArgumentParser
    {
        // ─────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the raw CLI arguments array into a deduplicated,
        /// sorted list of valid 1-based job indices.
        /// </summary>
        /// <param name="args">
        /// Raw argument array from Program.Main().
        /// Expected to contain a single token like "1-3" or "1;3".
        /// </param>
        /// <param name="maxJobs">
        /// Maximum valid index (number of configured jobs).
        /// Indices outside [1..maxJobs] are discarded.
        /// </param>
        /// <returns>
        /// Sorted, deduplicated list of valid indices.
        /// Empty list if no valid indices were found.
        /// </returns>
        public List<int> Parse(string[] args, int maxJobs)
        {
            if (args == null || args.Length == 0)
                return new List<int>();

            // Join all args in case the user typed spaces (e.g. "1 - 3")
            string token = string.Join("", args).Trim();

            if (string.IsNullOrEmpty(token))
                return new List<int>();

            List<int> indices;

            if (IsRange(token))
                indices = ParseRange(token);
            else if (IsList(token))
                indices = ParseList(token);
            else
                indices = ParseSingle(token);

            // Filter to valid range, deduplicate, and sort
            return indices
                .Where(i => ValidateIndex(i, maxJobs))
                .Distinct()
                .OrderBy(i => i)
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────
        // Format detection
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the token represents a range (contains a hyphen
        /// between two numbers, e.g. "1-3").
        /// Note: a leading minus sign for negative numbers is not a valid range.
        /// </summary>
        private static bool IsRange(string token)
        {
            // A range must have a hyphen that is not the first character
            int hyphenIndex = token.IndexOf('-');
            return hyphenIndex > 0 && hyphenIndex < token.Length - 1;
        }

        /// <summary>
        /// Returns true if the token represents a list (contains a semicolon,
        /// e.g. "1;3" or "1;2;3").
        /// </summary>
        private static bool IsList(string token)
        {
            return token.Contains(';');
        }

        // ─────────────────────────────────────────────────────────────
        // Parsing methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a range token ("1-3") into a list of consecutive indices [1, 2, 3].
        /// Returns an empty list if the token is malformed.
        /// </summary>
        private static List<int> ParseRange(string token)
        {
            string[] parts = token.Split('-');
            if (parts.Length != 2)
                return new List<int>();

            if (!int.TryParse(parts[0].Trim(), out int from) ||
                !int.TryParse(parts[1].Trim(), out int to))
                return new List<int>();

            if (from > to)
                return new List<int>();

            return Enumerable.Range(from, to - from + 1).ToList();
        }

        /// <summary>
        /// Parses a list token ("1;3") into a list of explicit indices [1, 3].
        /// Non-numeric parts are silently skipped.
        /// </summary>
        private static List<int> ParseList(string token)
        {
            return token
                .Split(';')
                .Select(part => part.Trim())
                .Where(part => int.TryParse(part, out _))
                .Select(int.Parse)
                .ToList();
        }

        /// <summary>
        /// Parses a single numeric token ("2") into a one-element list [2].
        /// Returns an empty list if the token is not a valid integer.
        /// </summary>
        private static List<int> ParseSingle(string token)
        {
            if (int.TryParse(token, out int index))
                return new List<int> { index };

            return new List<int>();
        }

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the given index is within the valid range [1..maxJobs].
        /// </summary>
        /// <param name="index">Index to validate.</param>
        /// <param name="maxJobs">Maximum valid index.</param>
        private static bool ValidateIndex(int index, int maxJobs)
        {
            return index >= 1 && index <= maxJobs;
        }
    }
}
