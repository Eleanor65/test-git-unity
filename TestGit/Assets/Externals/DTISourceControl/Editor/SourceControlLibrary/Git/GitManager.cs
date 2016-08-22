using System.Collections.Generic;
using System.Linq;
using DTI.SourceControl.ConsoleTools;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Git
{
    public class GitManager : ISourceControlManager
    {
        private readonly GitTools _tools = new GitTools();
        private readonly string _repositoryFolder;

        public GitManager(string directory)
        {
            _repositoryFolder = directory;
        }

        public void ShowOptionsWindow()
        {
            var window = EditorWindow.GetWindow<OptionsWindow>("Options");
            window.Tools = _tools;
            window.Show();
        }

        public void UpdateAll()
        {
            throw new System.NotImplementedException();
        }

        public void ShowCommitWindow()
        {
            throw new System.NotImplementedException();
        }

        public void ShowChooseBranchWindow()
        {
            var window = EditorWindow.GetWindow<BranchesWindow>("Choose branch");
            Fetch();
            window.Branches = GetBranches();
            window.Show();
        }

        //public void GetStatus()
        //{
        //    var cmd = new Cmd()
        //    {
        //        BaseDirectory = Application.dataPath,
        //        Command = EditorPrefs.GetString(GitTools.PATHKEY),
        //        Args = "status",
        //        Patterns = new []
        //        {
        //            "^(?<skip>On branch .+)$",
        //            "^(?<skip>Your branch is up-to-date with '.+'.)$",
        //            "^(?<skip>nothing to commit, working tree clean)$",
        //            //"^(?<skip>)$"
        //        }
        //    };

        //    var outResult = cmd.Run();
        //}

        public void Fetch()
        {
            var cmd = new Cmd()
            {
                BaseDirectory = _repositoryFolder,
                Command = EditorPrefs.GetString(_tools.PathKey),
                Args = "fetch",
            };
            cmd.Run();
        }

        public IEnumerable<Branch> GetBranches()
        {
            var cmd = new Cmd()
            {
                BaseDirectory = _repositoryFolder,
                Command = EditorPrefs.GetString(_tools.PathKey),
                Args = "branch -a",
                Patterns = new []
                {
                    "^(?<skip>[ ]+remotes/origin/HEAD -> origin/.+)$",

                    @"^(?<out>[*]?[ ]+\S+)$",
                    //@"^(?<out>[*]?[ ]+remotes/origin/\w+)$",
                    //"^(?<out>)$",
                }
            };
            var outResult = cmd.Run();

            var branches = outResult.Select(x => new GitBranch(x) as Branch);
            branches = GetDistinctBranches(branches);
            return branches;
        }

        private IEnumerable<Branch> GetDistinctBranches(IEnumerable<Branch> branches)
        {
            var distinctBranches = new List<Branch>();
            foreach (var branch in branches)
            {
                if (branches.Count(x => x.Name.Equals(branch.Name)) == 1)
                {
                    distinctBranches.Add(branch);
                    continue;
                }

                if (branch.Local)
                    distinctBranches.Add(branch);
            }
            return distinctBranches;
        }
    }
}