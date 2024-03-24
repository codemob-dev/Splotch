using HarmonyLib;

namespace Splotch.Loader
{
    /// <summary>
    /// Runs all of Splotch's patches.
    /// </summary>
    internal static class Patcher
    {
        public static Harmony harmony = new Harmony("com.splotch.splotchfixes");
        /// <summary>
        /// Runs all of Splotch's patches.
        /// </summary>
		public static void DoPatching()
        {
			harmony.PatchAll(typeof(Patches));
		}

        [HarmonyPatch]
        internal static class Patches
        {
	// put your patch here
        }
	    
	[HarmonyPatch]
        internal static class Patches
        {
	// second one for ease
        }
    }

}
