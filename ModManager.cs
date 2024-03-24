using Splotch.Loader.ModLoader;
using System.Collections.Generic;

namespace Splotch
{
    /// <summary>
    /// A class that contains every loaded mod.
    /// </summary>
    public static class ModManager
    {
        public static List<ModInfo> loadedMods = new List<ModInfo>();
        public static int modDisplayLimit = 20;

        /// <summary>
        /// Generates an info text describing the loaded mods. <c>modDisplayLimit</c> defines the maximum number of mods that will be displayed.
        /// 
        /// <example>
        /// For example:
        /// <code>
        /// ModManager.modDisplayLimit = 2;
        /// string info = ModManager.GetLoadedModsInfoText()
        /// </code>
        /// Could result in info equaling:
        /// <code>
        /// Splotch Mod Template by Codemob version 1.0
        /// BasicMod by WackyModer, Shad0w version 0.1.2
        /// ...and 3 others
        /// </code>
        /// </example>
        /// </summary>
        /// <returns>The info text</returns>
        public static string GetLoadedModsInfoText()
        {
            string modInfoString = "";
            int i = 0;
            foreach (var loadedMod in loadedMods)
            {
                if (i < modDisplayLimit)
                {
                    modInfoString += $"\n{loadedMod}";
                } else
                {
                    modInfoString += $"\n...and {loadedMods.Count - modDisplayLimit} others";
                    break;
                }
                i++;
            }
            Logger.Log(modInfoString);
            return modInfoString;
        }
    }
}
