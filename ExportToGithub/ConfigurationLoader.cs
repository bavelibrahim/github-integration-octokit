using System;
using System.IO;
using Newtonsoft.Json;

namespace ExportToGithub
{

    public class ConfigurationLoader
    {
        public static Configuration LoadConfiguration(string configFile)
        {
            try
            {
                using (StreamReader file = File.OpenText(configFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (Configuration)serializer.Deserialize(file, typeof(Configuration));
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Configuration file not found. Please create a 'config.json' file.");
                Environment.Exit(1);
                return null; // This is never reached due to Environment.Exit(1)
            }
        }
    }

}