using System;
using System.Net;
using System.Reflection;

namespace Splotch.Loader
{
    internal static class VersionChecker
    {
        public static void RunVersionChecker()
        {
            Version currentVersion = Assembly.GetCallingAssembly().GetName().Version;
            currentVersionString = currentVersion.ToString();
            currentVersionString = currentVersionString.Remove(currentVersionString.Length - 2);

            try
            {
            RetrieveVersionInfo();
            Version targetVersion = new Version((Config.LoadedSplotchConfig.nightly ? nightly : version) + ".0");
            updateNeeded = targetVersion > currentVersion;

            targetVersionString = targetVersion.ToString();
            targetVersionString = targetVersionString.Remove(targetVersionString.Length - 2);
            } catch (Exception) { }
        }

        private static string version;
        private static string nightly;

        public static string currentVersionString;
        public static string targetVersionString;
        public static bool updateNeeded = false;

        public static void RetrieveVersionInfo()
        {
            string result;
            using (WebClient client = new WebClient())
                result = client.DownloadString("https://raw.githubusercontent.com/commandblox/Splotch/master/version");

            string[] lines = result.Replace("\n\r", "\n").Replace("\r\n", "\n").Split('\n');
            version = lines[0].Split('=')[1];
            nightly = lines[1].Split('=')[1];
            Logger.Log(version);
        }
    }
}
