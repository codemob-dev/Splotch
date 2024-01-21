using HarmonyLib;
using Splotch.Loader.ModLoader;

namespace Splotch
{
    /// <summary>
    /// The base class that all mods must inherit
    /// </summary>
    public abstract class SplotchMod
    {
        public ModInfo modInfo { get; internal set; }
        public Harmony harmony { get; internal set; }

        internal void Setup(ModInfo modInfo)
        {
            this.modInfo = modInfo;
            harmony = new Harmony(modInfo.id);

        }
        /// <summary>
        /// Runs as soon as the mod is loaded after Splotch is done loading.
        /// </summary>
        public abstract void OnLoad();
    }
}
