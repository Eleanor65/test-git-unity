﻿namespace DTI.SourceControl
{
    public interface ISourceControlManager
    {
        void ShowOptionsWindow();

        void UpdateAll();

        void ShowCommitWindow();

        void ShowChooseBranchWindow();
    }
}