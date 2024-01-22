using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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

        internal static SplotchConfig LoadedSplotchConfig;

        private struct SplotchConfigContainer
        {
            public SplotchConfig splotchConfig;
        }

        internal static void CreateConfig() // This basically just creates the config, and if it already exists, we return back
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
                        consoleEnabled = false,
                        VerboseLoggingEnabled = false,
                    }
                };

                LoadedSplotchConfig = cont.splotchConfig;

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(cont);

                File.WriteAllText("splotch_config/splotchconfig.yaml", yaml);

            }


            return;

        }
    }
}
