using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportToGithub { 
public class Configuration
{
    public string token { get; set; }
    public string owner { get; set; }
    public string repoName { get; set; }
    public string branchName { get; set; }
    public string filePath { get; set; }
    public string commitMessage { get; set; }
}
}
