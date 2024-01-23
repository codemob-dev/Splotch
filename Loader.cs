using UnityEngine.SceneManagement;
using Splotch.Event;

namespace Splotch.Loader
{
    /// <summary>
    /// The main loader for Splotch. Called by Doorstop.
    /// </summary>
    public static class Loader
    {
        public static bool enteredScene = false;

        /// <summary>
        /// Called once when the first scene is loaded (After Unity finishes loading).
        /// Calls some of the other loaders.
        /// </summary>
        /// <param name="scene">The scene to be loaded</param>
        /// <param name="loadSceneMode">The scene mode</param>
        public static void OnEnterScene(Scene scene, LoadSceneMode loadSceneMode)
        {
            
            Logger.InitLogger();

            Logger.Log("Entering main menu");

            enteredScene = true;
            Patcher.DoPatching();
            ModLoader.ModLoader.LoadMods();
            EventManager.Load();
        }

        /// <summary>
        /// The main entrypoint for Splotch, called by Doorstop.
        /// </summary>
        public static void Main()   
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        /// <summary>
        /// Called by Unity when a scene loads. Runs any GUI modifications on that scene.
        /// </summary>
        /// <param name="scene">The scene to be loaded</param>
        /// <param name="loadSceneMode">The scene mode</param>
        private static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            // Execute patching after unity has finished it's startup and loaded at least the first game scene
            if (!enteredScene)
                OnEnterScene(scene, loadSceneMode);

            if (scene.name == "MainMenu")
                BaseGuiModifications.RunMainMenuModifications();
        }
    }
}