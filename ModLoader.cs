using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Reflection;
using System;

namespace Splotch.Loader.ModLoader
{
    /// <summary>
    /// Loads any mods in the <c>splotch_mods</c> folder.
    /// </summary>
    internal static class ModLoader
    {
        const string MOD_FOLDER_PATH = "splotch_mods";
        const string MOD_INFO_FILE_NAME = "modinfo.yaml";
        public static DirectoryInfo modFolderDirectory;
        /// <summary>
        /// Called by the <c>Loader</c>, loads all detected mods and logs any issues encountered during loading.
        /// </summary>
        internal static void LoadMods()
        {
            Logger.Log("Starting to load mods...");
            int modCountLoaded = 0;
            int modCountTot = 0;
            modFolderDirectory = Directory.CreateDirectory(MOD_FOLDER_PATH);
            foreach (DirectoryInfo modFolder in modFolderDirectory.GetDirectories())
            {
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
                            Logger.Log($"{data} loaded");
                            modCountLoaded++;
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

            Logger.Log($"Loaded {modCountLoaded}/{modCountTot} mods successfully!");
        }
    }

    /// <summary>
    /// An intermediate class that represents the format of the <c>modinfo.yaml</c> file.
    /// </summary>
    class DeserializedModInfo
    {
        public ModEntrypointData entrypoint { get; set; }
        public ModAttributesData attributes { get; set; }
        /// <summary>
        /// An internal class for the <c>entrypoint</c> section of the <c>modinfo.yaml</c> file.
        /// </summary>
        public class ModEntrypointData
        {
            public string dll { get; set; }
            public string className { get; set; }
        }

        /// <summary>
        /// An internal class for the <c>attributes</c> section of the <c>modinfo.yaml</c> file.
        /// </summary>
        public class ModAttributesData
        {
            public string id { get; set; }
            public string name { get; set; }

            public string description { get; set; } = "";
            public string version { get; set; } = "1.0";
            public string[] authors { get; set; } = { };
        }

        /// <summary>
        /// Converts the class into the final <c>ModInfo</c> format.
        /// </summary>
        /// <returns>The <c>ModInfo</c> representation of the class</returns>
        internal ModInfo ToModInfo()
        {
            return new ModInfo(entrypoint.dll, entrypoint.className, attributes.id, attributes.name, attributes.description, attributes.version, attributes.authors);
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

        public SplotchMod splotchMod;
        internal ModInfo(string dll, string className, string id, string name, string description, string version, string[] authors)
        {
            this.dll = dll;
            this.className = className;
            this.id = id;
            this.name = name;
            this.description = description;
            this.version = version;
            this.authors = authors;
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
                Assembly assembly = Assembly.LoadFile(dllAbsolutePath);
                Type assemblyEntrypoint = assembly.GetType(className);
                if (assemblyEntrypoint.BaseType == typeof(SplotchMod))
                {
                    splotchMod = (SplotchMod)Activator.CreateInstance(assemblyEntrypoint);
                    splotchMod.Setup(this);
                    splotchMod.OnLoad();

                    ModManager.loadedMods.Add(this);

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
                Logger.Error($"An error occurred while loading the mod {name}: \n{ex.Message}\n{ex.StackTrace}");
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