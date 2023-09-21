using System;
using System.IO;
using Newtonsoft.Json; // Ensure you have the Newtonsoft.Json NuGet package installed


namespace ExportToGithub
{
    class Program
    {

        static async Task Main(string[] args)
        {

            Configuration githubconfig = LoadConfiguration("C:/TrakSYS Github Integration/VS Project/ExportToGithub/ExportToGithub/config.json");

            var githubHelper = new GitHubClientHelper(githubconfig.token);

            var filePaths = githubHelper.GetFilesInFolder("C:/TrakSYS Github Integration/VS Project/filesForTesting");

            var result = await githubHelper.CreateOrUpdateAllPathAsync(githubconfig.owner, githubconfig.repoName, githubconfig.branchName, "C:/TrakSYS Github Integration/VS Project/filesForTesting", githubconfig.commitMessage);

            Console.WriteLine(result);
        
        }

        private static Configuration LoadConfiguration(string configFile)
        {
            try
            {
                using (StreamReader file = File.OpenText(configFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Console.WriteLine("Reading Configuration File.....");
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