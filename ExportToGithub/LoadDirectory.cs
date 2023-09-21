using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportToGithub
{
    internal class LoadDirectory
    {

        string currentDirectory;
        bool fileExists;

        public LoadDirectory()
        {
            // Gets the current directory you are in
            currentDirectory = Directory.GetCurrentDirectory();

            // Checks if the file exists
            fileExists = File.Exists(currentDirectory);
        }

        public void log() 
        {
            Console.WriteLine("Current Directory: " + currentDirectory);
            Console.WriteLine("File Exists: " + fileExists);
        }
    }
}
