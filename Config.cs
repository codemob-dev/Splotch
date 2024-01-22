using Splotch.Loader.ModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Splotch
{
    public static class Config
    {
        internal struct SplotchConfig
        {
            public bool splotchEnabled;
            public bool consoleEnabled;
            public bool VerboseLoggingEnabled;
        }

        internal static SplotchConfig LoadedSplotchConfig = new SplotchConfig
        {
            splotchEnabled = true,
            consoleEnabled = true,
            VerboseLoggingEnabled = true,
        };

        internal struct SplotchConfigContainer
        {
            public SplotchConfig splotchConfig;
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
                    splotchConfig = new SplotchConfig
                    { 
                        splotchEnabled = true,
                        consoleEnabled = true,
                        VerboseLoggingEnabled = true,
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
                        LoadedSplotchConfig.VerboseLoggingEnabled = deserializedData.splotchConfig.VerboseLoggingEnabled;
                    }
                }
                catch (Exception ex)
                {
                    // Logging hasn't been inited yet :P
                    UnityEngine.Debug.LogException(ex);
                }
            }


            return;

        }
    }
}
