// EasyLog/Helpers/LogFileNamer.cs — unchanged from v1.1
namespace EasyLog.Helpers
{
    public static class LogFileNamer
    {
        public const string DefaultExtension = ".json";

        public static string GetFileName(DateTime date, string dateFormat = "yyyy-MM-dd",
            string ext = DefaultExtension) => date.ToString(dateFormat) + ext;

        public static string GetFullPath(string logDirectory, DateTime date,
            string dateFormat = "yyyy-MM-dd", string ext = DefaultExtension) =>
            Path.Combine(logDirectory, GetFileName(date, dateFormat, ext));

        public static bool TryParseDateFromFileName(string fileName, out DateTime date,
            string dateFormat = "yyyy-MM-dd") =>
            DateTime.TryParseExact(
                Path.GetFileNameWithoutExtension(Path.GetFileName(fileName)),
                dateFormat, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date);
    }
}
