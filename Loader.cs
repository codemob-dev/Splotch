using UnityEngine.SceneManagement;
using System.Reflection;
using Splotch.Event;
using Splotch;
using UnityEngine;
using System;
using System.Diagnostics;

namespace Splotch.Loader
{
    /// <summary>
    /// The main loader for Splotch. Called by Doorstop.
    /// </summary>
    public static class Loader
    {

        struct SplotchConfigContainer
        {
            public string modName;
            public int someValue;
        }

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

            Logger.Log($"Entering main menu on version {VersionChecker.currentVersionString}");

            enteredScene = true;
            Patcher.DoPatching();
            ModLoader.ModLoader.LoadMods();
            EventManager.Load();

            GameObject obj = new GameObject("Unloader", new Type[] { typeof(UnLoader) });
        }

        /// <summary>
        /// The main entrypoint for Splotch, called by Doorstop.
        /// </summary>
        public static void Main()
        {
            Config.CreateConfigAndLoadSplotchConfig();

            if (!Config.LoadedSplotchConfig.splotchEnabled) 
                return;

            SceneManager.sceneLoaded += SceneLoaded;

            
        }

        internal static void GameExit()
        {
            ModLoader.ModLoader.UnloadMods();
            Logger.Log("Finished unloading!");
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

    class UnLoader : MonoBehaviour
    {
        public static UnLoader Instance
        {
            get;
            set;
        }

        void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
            Instance = this;
        }

        public void OnApplicationQuit()
        {
            Loader.GameExit();
        }
    }
}