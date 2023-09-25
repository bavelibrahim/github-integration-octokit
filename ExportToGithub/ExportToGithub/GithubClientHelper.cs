using System.Diagnostics;
using Octokit;

namespace ExportToGithub
{

    public class GitHubClientHelper
    {
        private readonly GitHubClient _githubClient;
        public string path_pullDirectory = "";

        public GitHubClientHelper(string token)
        {
            // Initialize the GitHub client with the provided access token
            _githubClient = new GitHubClient(new ProductHeaderValue("GitHubExporter"));
            _githubClient.Credentials = new Credentials(token);
        }

        /// <summary>
        /// Retrieves the SHA of the latest commit on a specific branch of a GitHub repository.
        /// </summary>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <param name="branchName">The name of the branch for which to retrieve the latest commit SHA.</param>
        /// <returns>The SHA of the latest commit on the specified branch, or an error message if an exception occurs.</returns>
        public async Task<string> GetLatestCommitShaAsync(string owner, string repoName, string branchName)
        {
            try
            {
                // Get the latest commit SHA of a specific branch in a repository
                var branch = await _githubClient.Repository.Branch.Get(owner, repoName, branchName);
                return branch.Commit.Sha;
            }
            catch (Exception ex)
            {
                // If an error occurs during the operation (e.g., branch doesn't exist),
                // return an error message containing the exception message.
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Creates or updates files in a GitHub repository with the provided content and commits the changes.
        /// </summary>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <param name="branchName">The name of the branch to update.</param>
        /// <param name="filePaths">A list of local file paths to be pushed.</param>
        /// <param name="commitMessage">The commit message for the changes.</param>
        /// <returns>A message indicating the status of the push operation.</returns>
        public async Task<string> CreateOrUpdateFilesAsync(string owner, string repoName, string branchName, List<string> filePaths, string commitMessage)
        {
            try
            {
                // Get the current commit SHA of the branch
                var currentCommit = await GetLatestCommitShaAsync(owner, repoName, branchName);

                // Create a new tree with the updated file(s)
                var newTree = new NewTree
                {
                    BaseTree = currentCommit, // Set the base tree SHA
                };

                // Iterate through each file to be pushed
                foreach (var file in filePaths)
                {
                    // Read the content of the file
                    var content = File.ReadAllText(file);

                    // Create a new tree item representing the file
                    var newTreeItem = new NewTreeItem
                    {
                        Path = file,
                        Mode = "100644", // File Mode
                        Content = content
                    };

                    // Add the new tree item to the new tree
                    newTree.Tree.Add(newTreeItem);
                }

                // Create the new tree in the repository
                var newTreeResponse = await _githubClient.Git.Tree.Create(owner, repoName, newTree);

                // Create a new commit using the new tree
                var newCommit = new NewCommit(commitMessage, newTreeResponse.Sha, currentCommit);
                var newCommitResponse = await _githubClient.Git.Commit.Create(owner, repoName, newCommit);

                // Update the branch reference to point to the new commit
                await _githubClient.Git.Reference.Update(owner, repoName, $"heads/{branchName}", new ReferenceUpdate(newCommitResponse.Sha));

                return "File(s) successfully pushed to GitHub.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves a list of file paths in the specified folder.
        /// </summary>
        /// <param name="folderPath">The local folder path to search for files.</param>
        /// <returns>A list of file paths found in the folder.</returns>
        public List<string> GetFilesInFolder(string folderPath)
        {
            try
            {
                // Use the Directory class to retrieve an array of file paths in the folder,
                // and convert it to a List<string> for ease of use.
                return Directory.GetFiles(folderPath).ToList();
            }
            catch (Exception ex)
            {
                // If an error occurs during the operation (e.g., folder doesn't exist),
                // log the error message and return an empty list.
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Pushes all files from a specified folder, including subfolders, to a GitHub repository while maintaining folder structure.
        /// </summary>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repoName">The name of the GitHub repository.</param>
        /// <param name="branchName">The name of the branch to update.</param>
        /// <param name="folderPath">The local folder path containing the files to be pushed.</param>
        /// <param name="commitMessage">The commit message for the changes.</param>
        /// <returns>A message indicating the status of the push operation.</returns>
        public async Task<string> CreateOrUpdateAllPathAsync(string owner, string repoName, string branchName, string folderPath, string commitMessage)
        {
            try
            {
                // Get the current commit SHA of the branch
                var currentCommit = await GetLatestCommitShaAsync(owner, repoName, branchName);

                // Traverse the folder recursively and get all file paths
                var filePaths = GetFilesInFolderRecursively(folderPath);

                // Create a new tree representing the changes
                var newTree = new NewTree
                {
                    BaseTree = currentCommit,
                };

                // Iterate through each file to be pushed
                foreach (var filePath in filePaths)
                {
                    // Read the file content
                    var content = File.ReadAllText(filePath);

                    // Calculate the relative path within the repository
                    var relativePath = GetRelativePath(folderPath, filePath);

                    // Create a new tree item representing the file
                    var newTreeItem = new NewTreeItem
                    {
                        Path = relativePath,
                        Mode = "100644", // File Mode
                        Content = content
                    };

                    // Add the new tree item to the new tree
                    newTree.Tree.Add(newTreeItem);
                }

                // Create the new tree in the repository
                var newTreeResponse = await _githubClient.Git.Tree.Create(owner, repoName, newTree);

                // Create a new commit using the new tree
                var newCommit = new NewCommit(commitMessage, newTreeResponse.Sha, currentCommit);
                var newCommitResponse = await _githubClient.Git.Commit.Create(owner, repoName, newCommit);

                // Update the branch reference to point to the new commit
                await _githubClient.Git.Reference.Update(owner, repoName, $"heads/{branchName}", new ReferenceUpdate(newCommitResponse.Sha));

                return "Files successfully pushed to GitHub.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Recursively retrieves a list of file paths within a specified folder and its subfolders.
        /// </summary>
        /// <param name="folderPath">The path of the folder to start the search from.</param>
        /// <returns>A List of string containing the full file paths found within the folder and its subfolders.</returns>
        private List<string> GetFilesInFolderRecursively(string folderPath)
        {
            var filePaths = new List<string>();

            // Add files in the current directory to the list
            filePaths.AddRange(Directory.GetFiles(folderPath));

            // Recursively explore subdirectories and add their files to the list
            foreach (var subdirectory in Directory.GetDirectories(folderPath))
            {
                filePaths.AddRange(GetFilesInFolderRecursively(subdirectory));
            }

            return filePaths;
        }

        /// <summary>
        /// Computes the relative path of a given full path with respect to a root path.
        /// </summary>
        /// <param name="rootPath">The root path used as a reference point.</param>
        /// <param name="fullPath">The full path for which the relative path is computed.</param>
        /// <returns>A string representing the relative path from the rootPath to the fullPath.</returns>
        private string GetRelativePath(string rootPath, string fullPath)
        {
            // Create Uri objects for the rootPath and fullPath
            var uri1 = new Uri(rootPath);
            var uri2 = new Uri(fullPath);

            // Use the MakeRelativeUri method to calculate the relative path
            return uri1.MakeRelativeUri(uri2).ToString();
        }

        /// <summary>
        /// Executes a Git command in a specified directory and captures the output and error messages.
        /// </summary>
        /// <param name="command">The Git command to execute (e.g., "pull", "commit").</param>
        /// <param name="directory">The working directory where the Git command should be executed.</param>
        public void RunGitCommand(string command)
        {
            Console.WriteLine("Please type in your working directory path: \n");
            var directory = Console.ReadLine();

            // Configure the process to run the Git command
            var processInfo = new ProcessStartInfo("git", command)
            {
                CreateNoWindow = true,              // Do not create a separate window for the process
                UseShellExecute = false,            // Do not use the system shell to execute the command
                RedirectStandardError = true,       // Redirect error messages to be captured
                RedirectStandardOutput = true,      // Redirect standard output to be captured
                WorkingDirectory = directory        // Set the working directory for the process
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                // Start the process to execute the Git command
                process.Start();

                // Read the standard output and error messages
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait for the process to complete
                process.WaitForExit();

                // Display the Git command's output
                Console.WriteLine("Git Output:");
                Console.WriteLine(output);

                // Display any error messages (if present)
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Git Log:");
                    Console.WriteLine(error);
                }
            }
        }
    }
}