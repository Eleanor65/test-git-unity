using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DTI.SourceControl.Git
{
    public class GitFileStatus : FileStatus
    {
        public GitFileStatus(string line, string repositoryPath)
        {
            var regex = new Regex(String.Format("^[ MADRCU?][ MDUA?][ ](?<{0}>.+)$", PATHGROUP));
            var match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                path = Path.Combine(repositoryPath, path);
                SetStatus(line);
                SetPath(path);
            }
        }

        public GitFileStatus(string path, Status status)
        {
            Status = status;
            SetPath(path);
        }

        private void SetStatus(string line)
        {
            var statusString = line.Substring(0, 2);
            switch (statusString)
            {
                case " D":
                    Status = Status.Deleted;
                    break;
                case "??":
                    Status = Status.Added;
                    break;
                case " M":
                    Status = Status.Modified;
                    break;
                case "DD":
                case "AU":
                case "UD":
                case "UA":
                case "DU":
                case "AA":
                case "UU":
                    Status = Status.Conflicted;
                    break;
            }
        }
    }
}