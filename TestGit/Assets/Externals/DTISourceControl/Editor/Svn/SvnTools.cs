using System.IO;
using UnityEditor;

namespace DTI.SourceControl.Svn
{
    public class SvnTools : ISourceConrolTools
    {
        public string PathKey { get { return "svnPath"; } }
        public string LoginKey { get { return "svnLogin"; } }
        public string PasswordKey { get { return "svnPassword"; } }

        public string Executable
        {
            get
            {
                if (RuntimePlatformHelper.GetCurrentPlatform() == RuntimePlatform.Windows)
                    return "svn.exe";
                else
                    return "svn";
            }
        }

        public string SvnDirectory
        {
            get { return Path.GetDirectoryName(EditorPrefs.GetString(PathKey)); }
        }
    }
}
