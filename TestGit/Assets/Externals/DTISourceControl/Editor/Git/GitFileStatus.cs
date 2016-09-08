using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

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
                if (path[0].Equals('\"') && path[path.Length - 1].Equals('\"'))
                    path = path.Substring(1, path.Length - 2);
                path = Path.Combine(repositoryPath, path);
                if (!path.Replace("/", "\\").StartsWith(Application.dataPath.Replace("/", "\\"))) 
                    return;

                SetPath(path);
                SetStatus(line);
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