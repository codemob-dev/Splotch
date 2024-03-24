using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Reflection;
using System;
using HarmonyLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;

namespace Splotch.Loader.ModLoader
{
    /// <summary>
    /// Loads any mods in the <c>splotch_mods</c> folder.
    /// </summary>
    internal static class ModLoader
    {
        static string MOD_FOLDER_PATH = "splotch_mods";
        static readonly string MOD_INFO_FILE_NAME = "modinfo.yaml";
        static string UNZIPPED_MOD_TEMP_FOLDER = @"splotch_mods\temp"; // Can't have as readonly because thunderstore

        public static DirectoryInfo modFolderDirectory;
        public static DirectoryInfo modFolderTempDirectory;
        /// <summary>
        /// Called by the <c>Loader</c>, loads all detected mods and logs any issues encountered during loading.
        /// </summary>
        internal static void LoadMods()
        {
            Logger.Log("Starting to load mods...");
            if (Loader.ModPath != null)
            {
                Logger.Log($"Loading mods from Thunderstore mod manager path {Loader.ModPath}");
                MOD_FOLDER_PATH = Loader.ModPath;
                UNZIPPED_MOD_TEMP_FOLDER = Loader.ModPath + "\\temp";
            }
            int modCountLoaded     = 0;
            int modCountTot        = 0;
            modFolderDirectory     = Directory.CreateDirectory(MOD_FOLDER_PATH);
            modFolderTempDirectory = Directory.CreateDirectory(UNZIPPED_MOD_TEMP_FOLDER);

            ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
            foreach (FileInfo modZipFile in modFolderDirectory.GetFiles())
            {
                if (modZipFile.Extension.ToLower() == ".zip")
                {
                    Logger.Debug($"Unzipped mod {modZipFile.Name} detected!");

                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(modZipFile.FullName, modFolderTempDirectory.CreateSubdirectory(
                        Path.GetFileNameWithoutExtension(modZipFile.Name)
                        ).FullName, null);
                }
            }

			Dictionary<string, ModInfo> loadedMods = new Dictionary<string, ModInfo>();

			foreach (DirectoryInfo modFolder in modFolderDirectory.GetDirectories().AddRangeToArray(modFolderTempDirectory.GetDirectories()))
            {
                if (modFolder.FullName == modFolderTempDirectory.FullName) continue;

                modCountTot++;
                string modFolderPath = modFolder.FullName;
                string modInfoPath = Path.Combine(modFolderPath, MOD_INFO_FILE_NAME);
                if (File.Exists(modInfoPath))
                {
                    try
                    {
                        ModInfo data;
                        using (StreamReader reader = new StreamReader(modInfoPath))
                        {
                            IDeserializer deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                            DeserializedModInfo deserializedData = deserializer.Deserialize<DeserializedModInfo>(reader);
                            data = deserializedData.ToModInfo();
                        }

                        bool loadSuccess = data.LoadMod(modFolderPath);
                        if (loadSuccess)
                        {
                            loadedMods.Add(data.id, data);
                            //Debug.Log($"{data} loaded");
                        }
                        else
                        {
                            Logger.Warning($"Failed to load {data.name}");
                            //Debug.LogWarning($"Failed to load {data.name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"An error occurred while loading the modinfo.yaml file of {modFolder.Name}: \n{ex.Message}\n{ex.StackTrace}");
                        //Debug.LogError($"An error occurred while loading the modinfo.yaml file of {modFolder.Name}: \n{ex.Message}\n{ex.StackTrace}");
                    }

                }
                else
                {
                    Logger.Warning($"Invalid mod folder {modFolder.Name}!");
                    //Debug.LogWarning($"Invalid mod folder {modFolder.Name}!");
                }
            }

			void LoadWithDependencies(ModInfo mod)
			{
				bool modHasAllDep = true;

				foreach (string depId in mod.dependencies)
				{
                    if (loadedMods.TryGetValue(depId, out ModInfo depMod))
                    {
                        if (depMod.initalized) continue;
                        LoadWithDependencies(depMod);
                    }
                    else
                    {
                        modHasAllDep = false;
                        Logger.Warning($"{mod.name} is missing dependency '{depId}'");
                    }
				}

                if (!modHasAllDep) return;

				Logger.Log($"{mod} loaded");
				modCountLoaded++;

                mod.FinishInit();
			}

			foreach (KeyValuePair<string, ModInfo> pair in loadedMods)
            {
                ModInfo mod = pair.Value;
                if (mod.initalized) continue;
                LoadWithDependencies(mod);
			}

            Logger.Log($"Loaded {modCountLoaded}/{modCountTot} mods successfully!");

	    // load patches
	    // Splotch.Patches.SplotchPatches.ApplyPatches(); // someone please fix my code bad please im beg of you!
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                file.IsReadOnly = false;
                File.SetAttributes(file.FullName, FileAttributes.Normal);
                file.Delete();
            }
            baseDir.Delete();
        }

        internal static void UnloadMods()
        {
            Logger.Log("Unloading mods...");
            foreach (var loadedMod in ModManager.loadedMods)
            {
                loadedMod.splotchMod.OnUnload();
                Logger.Debug($"Unloaded {loadedMod.name}");
            }
            Logger.Debug("Clearing temp...");
            RecursiveDelete(modFolderTempDirectory);
        }
    }

    /// <summary>
    /// An intermediate class that represents the format of the <c>modinfo.yaml</c> file.
    /// </summary>
    class DeserializedModInfo
    {
        public ModEntrypointData Entrypoint { get; set; }
        public ModAttributesData Attributes { get; set; }
        /// <summary>
        /// An internal class for the <c>entrypoint</c> section of the <c>modinfo.yaml</c> file.
        /// </summary>
        public class ModEntrypointData
        {
            public string Dll { get; set; }
            public string ClassName { get; set; }
        }

        /// <summary>
        /// An internal class for the <c>attributes</c> section of the <c>modinfo.yaml</c> file.
        /// </summary>
        public class ModAttributesData
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public string Description { get; set; } = "";
            public string Version { get; set; } = "1.0";
            public string[] Authors { get; set; } = { };
            public string[] Dependencies { get; set; } = { };
		}

        /// <summary>
        /// Converts the class into the final <c>ModInfo</c> format.
        /// </summary>
        /// <returns>The <c>ModInfo</c> representation of the class</returns>
        internal ModInfo ToModInfo()
        {
            return new ModInfo(Entrypoint.Dll, Entrypoint.ClassName, Attributes.Id, Attributes.Name, Attributes.Description, Attributes.Version, Attributes.Authors, Attributes.Dependencies);
        }
    }

    /// <summary>
    /// The class that stores all data about a <c>SplotchMod</c>.
    /// </summary>
    public class ModInfo
    {
        public string dll;
        public string className;
        public string id;
        public string name;
        public string description;
        public string version;
        public string[] authors;
        public string[] dependencies;

		public SplotchMod splotchMod;
        public Assembly assembly;
        internal bool initalized = false;

        internal ModInfo(string dll, string className, string id, string name, string description, string version, string[] authors, string[] dependencies)
        {
            this.dll = dll;
            this.className = className;
            this.id = id;
            this.name = name;
            this.description = description;
            this.version = version;
            this.authors = authors;
            this.dependencies = dependencies;
		}

		/// <summary>
		/// Called after every dependency loaded and there was no error.
		/// </summary>
		internal void FinishInit()
        {
			initalized = true;
			splotchMod.OnLoad();
			ModManager.loadedMods.Add(this);
		}

        /// <summary>
        /// Attempts to load the mod from the metadata using the <c>dll</c> and <c>className</c> fields to determine the entrypoint.
        /// </summary>
        /// <param name="modFolder">The folder the mod is contained in.</param>
        /// <returns><c>true</c> if the loading was successful and <c>false</c> if there was an error.</returns>
        internal bool LoadMod(string modFolder)
        {
            try
            {
                string dllAbsolutePath = Path.Combine(modFolder, dll);
                Logger.Debug($"Loading {dllAbsolutePath}");

                assembly = Assembly.LoadFrom(dllAbsolutePath);

                Logger.Debug($"Loaded {assembly}");
                Type assemblyEntrypoint = assembly.GetType(className);
                if (assemblyEntrypoint == null)
                {
                    Logger.Error($"{className} is not a valid class! Valid classes are: {string.Join<Type>(", ", assembly.GetTypes())}, if this is wrong, try changing the name of your assembly (your dll)");
                    return false;
                }

                if (assemblyEntrypoint.BaseType == typeof(SplotchMod))
                {
                    splotchMod = (SplotchMod)Activator.CreateInstance(assemblyEntrypoint);
                    splotchMod.Setup(this);

                    return true;
                }
                else
                {
                    Logger.Error($"The main class in {name} does not extend SplotchMod!");
                    //Debug.LogError($"The main class in {name} does not extend SplotchMod!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while loading the mod {name}: \n{ex.Message}\n{ex.StackTrace}\n{ex.GetType()}");
                //Debug.LogError($"An error occurred while loading the mod {name}: \n{ex.Message}\n{ex.StackTrace}");
            }
            return false;
        }

        /// <summary>
        /// Returns a string representation of the info in the format of "<c>this.name</c> by <c>string.Join(", ", this.authors)</c> version <c>this.version</c>"
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return $"{name} by {string.Join(", ", authors)} version {version}";
        }
    }
}
