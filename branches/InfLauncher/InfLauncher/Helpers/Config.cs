using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace InfLauncher.Helpers
{
    /// <summary>
    /// Parses the configuration file.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The path of the configuration file.
        /// </summary>
        public static string Filename = @"config.xml";

        public string AccountsUrl { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            using(XmlReader reader = XmlReader.Create(new FileStream(Filename, FileMode.Open)))
            {
                // Account Server configuration
                reader.ReadToFollowing("accounts");


                // Asset Download configuration
                reader.ReadToFollowing("assets");

                // Installation path
                reader.ReadToFollowing("install");
            }

            return true;
        }
    }
}
