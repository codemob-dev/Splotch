using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Splotch
{
    public static class Config
    {
        /// <summary>
        /// Don't touch it, it somehow works.
        /// </summary>
        internal struct SplotchConfigStruct
        {
            public bool splotchEnabled;
            public bool consoleEnabled;
            public bool verboseLoggingEnabled;
        }

        internal static SplotchConfigStruct LoadedSplotchConfig = new SplotchConfigStruct
        {
            splotchEnabled = true,
            consoleEnabled = true,
            verboseLoggingEnabled = true,
        };

        internal struct SplotchConfigCont
        {
            public SplotchConfigStruct splotchConfig;
        }

        /// <summary>
        /// Loads config for Splotch. If the config folder OR file doesn't exist, then it just creates them
        /// 
        /// So far we have:
        /// 
        /// verbose logging (debug logs)
        /// console being enabled
        /// Splotch being enabled
        /// </summary>
        internal static void CreateConfigAndLoadSplotchConfig() // This basically just creates the config, and if it already exists, we return back
        {
            // WE CANNOT USE LOGGER HERE
            // WE CANNOT USE LOGGER HERE
            // WE CANNOT USE LOGGER HERE
            // WE CANNOT USE LOGGER HERE

            if (!Directory.Exists("splotch_config"))
            {
                Directory.CreateDirectory("splotch_config");
            }

            if (!File.Exists("splotch_config/splotchconfig.yaml"))
            {
                File.Create("splotch_config/splotchconfig.yaml");


                SplotchConfigCont cont = new SplotchConfigCont
                {
                    splotchConfig = new SplotchConfigStruct
                    {
                        splotchEnabled = true,
                        consoleEnabled = true,
                        verboseLoggingEnabled = false,
                    }
                };

                LoadedSplotchConfig = cont.splotchConfig;

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yamlToWrite = serializer.Serialize(cont);

                File.WriteAllText("splotch_config/splotchconfig.yaml", yamlToWrite);

            }
            else
            {

                try {

                    using (StreamReader reader = new StreamReader("splotch_config/splotchconfig.yaml"))
                    {
                        IDeserializer deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                        SplotchConfigCont deserializedData = deserializer.Deserialize<SplotchConfigCont>(reader);

                        LoadedSplotchConfig.splotchEnabled = deserializedData.splotchConfig.splotchEnabled;
                        LoadedSplotchConfig.consoleEnabled = deserializedData.splotchConfig.consoleEnabled;
                        LoadedSplotchConfig.verboseLoggingEnabled = deserializedData.splotchConfig.verboseLoggingEnabled;
                    }
                }
                catch (YamlException ex)
                {
                    Debug.LogException(ex);
                }
                catch (Exception ex)
                {
                    // Logging hasn't been inited yet :P
                    Debug.LogException(ex);
                }
            }


            return;

        }
    
        

        /// <summary>
        /// You pass in a struct or a class to the first param with typeof(YourClassOrStruct), then in the second param you pass in your default config, this is what your config file will be created with. 
        /// In the final param you pass in your mod name AND NAME to dictate the config file name.
        /// </summary>
        /// <param name="ConfigStruct"></param>
        /// <param name="DefaultConfig"></param>
        /// <param name="ModName"></param>
        /// <returns>The Yaml object. It can be converted to the config struct or class you passed in</returns>
        public static object CreateOrLoadConfig(Type ConfigStruct, object DefaultConfig, string ModName)
        {
            if (!File.Exists($"splotch_config/{ModName}.yaml"))
            {
                var ugh = File.Create($"splotch_config/{ModName}.yaml");
                ugh.Close();
                try
                {

                    Logger.Log("a");
                    var serializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    Logger.Log("a");

                    var yamlToWrite = serializer.Serialize(DefaultConfig);

                    Logger.Log(yamlToWrite);
                    Logger.Log("a");
                    System.Threading.Thread.Sleep(500);
                    File.WriteAllText($"splotch_config/{ModName}.yaml", yamlToWrite);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }

                return DefaultConfig;
            }
            else
            {
                try
                {
                    object deserializedData;

                    StreamReader reader = new StreamReader($"splotch_config/{ModName}.yaml");

                    Logger.Log("aaaaa");
                    // Deserialize
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    Logger.Log("aaaaa");


                    var yml = reader.ReadToEnd();

                    deserializedData = deserializer.Deserialize(yml, ConfigStruct);
                    Logger.Log("aaaaa");

                    reader.Close();

                    return deserializedData;
                }
                catch (YamlException ex)
                {
                    Logger.Error($"YAML Exception: {ex.Message}\nLine: {ex.Start.Line}, Column: {ex.Start.Column}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception during deserialization: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }

            return null;
        }
    }
}
