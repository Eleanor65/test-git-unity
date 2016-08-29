namespace DTI.SourceControl.Git
{
    internal class GitTools : ISourceConrolTools
    {
        public string PathKey { get { return "gitPath"; } }
        public string LoginKey { get { return "gitLogin"; } }
        public string PasswordKey { get { return "gitPassword"; } }

        public string Executable
        {
            get
            {
                if (RuntimePlatformHelper.GetCurrentPlatform() == RuntimePlatform.Windows)
                {
                    return "git.exe";
                }
                else
                {
                    return "git";
                }
            }
        }

    }
}