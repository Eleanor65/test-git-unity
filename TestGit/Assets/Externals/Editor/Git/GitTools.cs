namespace DTI.SourceControl.Git
{
    internal static class GitTools
    {
        public const string PATHKEY = "gitPath";
        public const string LOGINKEY = "gitLogin";
        public const string PASSWORDKEY = "gitPassword";

        public static string Git
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