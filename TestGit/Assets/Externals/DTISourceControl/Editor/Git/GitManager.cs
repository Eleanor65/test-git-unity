using System;
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
        private readonly string _git;

        public GitManager(string directory)
        {
            _repositoryFolder = directory;
            _git = EditorPrefs.GetString(_tools.PathKey);
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
            window.OnChooseBranch = OnChooseBranch;
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
            var cmd = GetCmd("fetch");
            cmd.Run();
        }

        public IEnumerable<Branch> GetBranches()
        {
            var cmd = GetCmd("branch -a", new[]
            {
                "^(?<skip>[ ]+remotes/origin/HEAD -> origin/.+)$",

                "^(?<out>[*]?[ ]+.+)$",
                //@"^(?<out>[*]?[ ]+remotes/origin/\w+)$",
                //"^(?<out>)$",
            });
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

        private void CheckoutBranch(Branch branch)
        {
            Debug.Log(123);
            var cmd = GetCmd("checkout " + branch.FullName, new[]
            {
                "^(?<error>error: Your local changes to the following files would be overwritten by checkout:)$",
                @"^(?<error>\s+.*)$",
                "^(?<error>Please commit your changes or stash them before you can switch branches[.])$",
                "^(?<error>Aborting)$",

                "^(?<out>Branch .+ set up to track remote branch .+ from origin[.])$",
                "^(?<out>Switched to a new branch '.+')$",
                "^(?<out>Switched to branch '.+')$",
                "^(?<out>Your branch is up-to-date with '.+'[.])$",
            });
            var outResult = cmd.Run();
            var str = String.Join(" ", outResult.ToArray());
            Debug.Log(str);
            EditorUtility.DisplayDialog("Changed branch successfully.", str, "Ok");
        }

        private IEnumerable<FileStatus> GetStatus()
        {
            var cmd = GetCmd("status -s", new[] {"^(?<out>[ MADRCU?][ MDUA?][ ].+)$"});
            var outResult = cmd.Run();
            var fileStatusList = outResult.Select(x => new GitFileStatus(x) as FileStatus);

            return fileStatusList;
        }

        private Cmd GetCmd(string args, string[] patterns)
        {
            return new Cmd()
            {
                BaseDirectory = _repositoryFolder,
                Command = _git,
                Args = args,
                Patterns = patterns
            };
        }

        private Cmd GetCmd(string args)
        {
            return new Cmd()
            {
                BaseDirectory = _repositoryFolder,
                Command = _git,
                Args = args
            };
        }

        public void OnChooseBranch(BranchesWindow window)
        {
            if (window.SelectedBranch == window.CurrentBranch)
            {
                EditorUtility.DisplayDialog("Already on " + window.SelectedBranch.Name,
                        String.Format("You are already on branch \"{0}\".", window.SelectedBranch.Name), "Ok");
                return;
            }

            CheckoutBranch(window.SelectedBranch);
        }
    }
}