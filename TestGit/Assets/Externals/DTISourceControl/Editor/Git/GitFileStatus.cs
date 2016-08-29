using System;
using System.Text.RegularExpressions;
using DTI.SourceControl;

namespace DTI.SourceControl.Git
{
    public class GitFileStatus : FileStatus
    {
        public GitFileStatus(string line)
        {
            var regex = new Regex(String.Format("^[ MADRCU?][ MDUA?][ ](?{0}.+)$", PATHGROUP));
            var match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                SetStatus(line);
                SetPath(path);
                return;
            }
        }

        private void SetStatus(string line)
        {

        }
    }
}