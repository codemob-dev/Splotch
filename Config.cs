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

        internal struct SplotchConfigContainer
        {
            public SplotchConfigStruct splotchConfig;
        }

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


                SplotchConfigContainer cont = new SplotchConfigContainer
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
                        SplotchConfigContainer deserializedData = deserializer.Deserialize<SplotchConfigContainer>(reader);

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
    }
}
