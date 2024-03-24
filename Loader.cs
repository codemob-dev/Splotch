using UnityEngine.SceneManagement;
using System.Reflection;
using Splotch.Event;
using Splotch;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using HarmonyLib;
using MonoMod.Utils;

namespace Splotch.Loader
{
    /// <summary>
    /// The main loader for Splotch. Called by Doorstop.
    /// </summary>
    public static class Loader
    {
        public static bool BepInExPresent { get { return Directory.Exists(@"BepInEx\core\"); } }
        public static string ModPath = null;

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

            string[] commandLineArgs = Environment.GetCommandLineArgs();

            foreach (string arg in commandLineArgs)
            {
                Console.WriteLine(arg);
            }

            enteredScene = true;
            Patcher.DoPatching();
            ModLoader.ModLoader.LoadMods();
            EventManager.Load();

            GameObject obj = new GameObject("Unloader", new Type[] { typeof(UnLoader) });
            Logger.Debug("Finished main menu loading!");
        }

        public static void LoadBepInEx()
        {
            string path = @"BepInEx\core\BepInEx.Preloader.dll";


            // Trick BepInEx to think it was invoked with doorstop
            string actualInvokePath = Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH");
            Environment.SetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH", Path.Combine(Directory.GetCurrentDirectory(), path));


            Assembly bepInExPreloader = Assembly.LoadFrom(path);

            if (bepInExPreloader != null)
            {
                MethodInfo entrypointMethod = bepInExPreloader.GetType("BepInEx.Preloader.Entrypoint", true)
                    .GetMethod("Main");

                entrypointMethod.Invoke(null, null);
            }

            Environment.SetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH", actualInvokePath);
        }

        /// <summary>
        /// The main entrypoint for Splotch, called by Doorstop.
        /// </summary>
        public static void Main()
        {
            // Test for the "--begone-splotch" command line arg so we can run vanilla

            string[] commandLineArgs = Environment.GetCommandLineArgs();

            bool CustomPath = false;
            foreach (string arg in commandLineArgs)
            {
                if (CustomPath)
                {
                    ModPath = arg;
                }

                CustomPath = false;

                // I have no clue why, it just breaks randomly and inoften when this is not .Contains()
                if (arg.Contains("--begone-splotch"))
                {
                    return;
                }

                if (arg.Contains("--splotch-mods-dir"))
                {
                    CustomPath = true;
                }
            }
            if (BepInExPresent)
                LoadBepInEx();

            Config.CreateConfigAndLoadSplotchConfig();

            if (!Config.LoadedSplotchConfig.splotchEnabled) 
                return;

            // BepInEx does some shenanigans with assembly loading so by using reflection to run unity
            // methods we can avoid loading assemblies before bepinex
            typeof(Loader).Assembly.GetType($"Splotch.Loader.BepInExLoader")
                .GetMethod(nameof(BepInExLoader.LoadBepInExUnstable))
                .Invoke(null, null);
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
        public static void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
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

    class BepInExLoader
    {
        public static void LoadBepInExUnstable()
        {
            SceneManager.sceneLoaded += Loader.SceneLoaded;
        }
    }
}
