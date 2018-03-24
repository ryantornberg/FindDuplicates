using System.Configuration;

namespace FindDuplicates
{
    static class AppSettingsReader
    {
        public static string SourceFolder()
        {
            return ConfigurationManager.AppSettings["SourceFolder"];
        }
    }
}