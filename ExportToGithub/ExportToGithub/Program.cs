using System;
using System.IO;
using Newtonsoft.Json; // Ensure you have the Newtonsoft.Json NuGet package installed
using Octokit;

namespace ExportToGithub
{
    class Program
    {

        ConfigurationLoader configurationLoader;


        static async Task Main(string[] args)
        {

            Configuration githubconfig = LoadConfiguration("C:/TrakSYS Github Integration/VS Project/ExportToGithub/ExportToGithub/config.json");

            var githubHelper = new GitHubClientHelper(githubconfig.token);

            var client = new GitHubClient(new ProductHeaderValue("GithubAPI"));

            var tokenAuth = new Credentials(githubconfig.token);
            client.Credentials = tokenAuth;

            // Create a pull request and wait for it to be merged
            // await githubHelper.CreateAndMonitorPullRequest(client, githubconfig.owner, githubconfig.repoName, githubconfig.branchName);
            
            // Create a fetch request
            githubHelper.RunGitCommand("pull");

            //var filePaths = githubHelper.GetFilesInFolder("C:/TrakSYS Github Integration/VS Project/filesForTesting");

            //var result = await githubHelper.CreateOrUpdateAllPathAsync(githubconfig.owner, githubconfig.repoName, githubconfig.branchName, "C:/TrakSYS Github Integration/VS Project/filesForTesting", githubconfig.commitMessage);

            //Console.WriteLine("Pull request merged. Changes pulled successfully.");
            //Console.WriteLine("Fetch request Successful. Changes fetched.");

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