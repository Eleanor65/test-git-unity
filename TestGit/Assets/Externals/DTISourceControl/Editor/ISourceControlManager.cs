namespace DTI.SourceControl
{
    public interface ISourceControlManager
    {
        void ShowOptionsWindow();

        void UpdateAll();

        void ShowCommitWindowAll();

        void ShowCommitWindowSelected();

        void ShowChooseBranchWindow();
    }
}