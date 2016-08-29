using System;
using System.Text.RegularExpressions;

namespace DTI.SourceControl.Svn
{
    public class SvnFileStatus : FileStatus
    {
        public SvnFileStatus(string line)
        {
            var regex = new Regex(String.Format("^([?!DAMC][ ]+)(?<{0}>[^ ].*)$", PATHGROUP));
            var match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                SetStatus(line);
                SetPath(path);
                return;
            }

            regex = new Regex(String.Format("^svn: warning: W155010: The node '(?<{0}>.+)' was not found[.]$", PATHGROUP));
            match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                Status = Status.NotFound;
                SetPath(path);
                return;
            }

            SetPath(line);
        }

        private void SetStatus(string line)
        {
            switch (line[0])
            {
                case '?':
                    Status = Status.NotUnderVC;
                    break;
                case '!':
                    Status = Status.Missing;
                    break;
                case 'D':
                    Status = Status.Deleted;
                    break;
                case 'A':
                    Status = Status.Added;
                    break;
                case 'M':
                    Status = Status.Modified;
                    break;
                case 'C':
                    Status = Status.Conflicted;
                    break;
            }
        }

    }
}