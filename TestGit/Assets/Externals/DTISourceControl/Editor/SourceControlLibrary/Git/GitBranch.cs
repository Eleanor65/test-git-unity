using System.Text.RegularExpressions;
using DTI.SourceControl;

namespace DTI.SourceControl.Git
{
    public class GitBranch : Branch
    {
        public GitBranch(string line)
        {
            if (line.StartsWith("*"))
                Current = true;

            var regex = new Regex(@"^[ ]+remotes/origin/(?<name>\S+)$");
            var match = regex.Match(line);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                FullName = "origin/" + Name;
                Local = false;
                return;
            }

            regex = new Regex(@"^[*]?[ ]+(?<name>\S+)$");
            match = regex.Match(line);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                FullName = Name;
                Local = true;
            }
        }
    }
}