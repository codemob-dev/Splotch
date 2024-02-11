using HarmonyLib;
using Splotch.Loader.ModLoader;

namespace Splotch
{
    /// <summary>
    /// The base class that all mods must inherit
    /// </summary>
    public abstract class SplotchMod
    {
        public ModInfo ModInfo { get; internal set; }
        public Harmony Harmony { get; internal set; }

        internal void Setup(ModInfo modInfo)
        {
            this.ModInfo = modInfo;
            Harmony = new Harmony(modInfo.id);

        }
        /// <summary>
        /// Runs as soon as the mod is loaded after Splotch is done loading.
        /// </summary>
        public abstract void OnLoad();
        public void OnUnload()
        {

        }
    }
}
