using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DTI.SourceControl.ConsoleTools;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Git
{
    public class GitManager : SourceControlManagerBase
    {
        private readonly GitTools _tools = new GitTools();
        private readonly string _repositoryFolder;
        private readonly string _git;

        public GitManager(string directory)
        {
            _repositoryFolder = directory;
            _git = EditorPrefs.GetString(_tools.PathKey);
        }

        public override void ShowOptionsWindow()
        {
            var window = EditorWindow.GetWindow<OptionsWindow>("Options");
            window.Tools = _tools;
            window.Show();
        }

        public override void UpdateAll()
        {
            var cmd = GetCmd("pull", new[]
            {
                "^(?<out>Already up-to-date[.])$",
                @"^(?<out>\s+\d+ files? changed,?(\s+\d+ insertions?[(][+][)])?,?(\s+\d+ deletions?[(][-][)])?)$",

                @"^(?<skip>remote: Counting objects: .*)$",
                "^(?<skip>remote: Compressing objects: .*)$",
                "^(?<skip>remote: Total .*)$",
                "^(?<skip>Unpacking objects: .*)$",
                "^(?<skip>From .*)$",
                @"^(?<skip>\s+.+ .+ -> .+)$",
                "^(?<skip>Updating .+)$",
                "^(?<skip>Fast-forward)$",
                @"^(?<skip>\s+.+ [|] \d+ [+-]+)$",
                @"^(?<skip>\s+(create|delete) mode .+)$",

                "^(?<error>error: Your local changes to the following files would be overwritten by merge:)$",
                @"^(?<error>\s+.*)$",
                "^(?<error>Please commit your changes or stash them before you can merge[.])$",
                "^(?<error>Aborting)$",
            });
            var outResult = cmd.Run();
            EditorUtility.DisplayDialog("Update results", String.Join("\n", outResult.ToArray()), "Ok");
        }

        public override void ShowCommitWindowAll()
        {
            UnstageAll();
            var statusList = GetStatus();
            GetAndShowCommitwindow(statusList);
        }

        public override void ShowCommitWindowSelected()
        {
            var selected = GetSelectedAssetPaths();
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Error",
                    "No assets were chosen to commit. Choose some asset/assets to commit.", "OK");
                return;
            }

            var statusList = GetStatus(selected);
            GetAndShowCommitwindow(statusList);
        }

        public override void ShowChooseBranchWindow()
        {
            Fetch();
            var branches = GetBranches();
            if (!branches.Any())
            {
                EditorUtility.DisplayDialog("No branches.", "There are no branches exept for this one.", "Ok");
                return;
            }

            var window = EditorWindow.GetWindow<BranchesWindow>("Choose branch");
            window.Branches = branches;
            window.OnChooseBranch = OnChooseBranch;
            window.Show();
        }

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

                public void OnChooseBranch(BranchesWindow window)
        {
            if (window.SelectedBranch == window.CurrentBranch)
            {
                EditorUtility.DisplayDialog("Already on " + window.SelectedBranch.Name,
                        String.Format("You are already on branch \"{0}\".", window.SelectedBranch.Name), "Ok");
                return;
            }

            CheckoutBranch(window.SelectedBranch);
            window.Close();
        }

        private void CheckoutBranch(Branch branch)
        {
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
            EditorUtility.DisplayDialog("Changed branch successfully.", String.Join("\n", outResult.ToArray()), "Ok");
        }

        private void UnstageAll()
        {
            var cmd = GetCmd("reset HEAD .", new[]
            {
                "^(?<skip>Unstaged changes after reset:)$",
                @"^(?<skip>\w\s+.+)$",

                "^(?<error>error: .+)$",
            });
        }

        private IEnumerable<FileStatus> GetStatus(params string[] paths)
        {
            var args = "status -s " + AddQuatationMarks(Application.dataPath);

            var cmd = GetCmd(args, new[] {"^(?<out>[ MADRCU?][ MDUA?][ ].+)$"});
            var outResult = cmd.Run();
            var fileStatusList = outResult.Select(x => new GitFileStatus(x, _repositoryFolder) as FileStatus).ToList();
            fileStatusList.RemoveAll(x => String.IsNullOrEmpty(x.FullPath));

            fileStatusList = GetAddedFilesInFolders(fileStatusList);

            if (paths.Any())
                fileStatusList = fileStatusList.Where(x => paths.Any(y=> x.FullPath.Replace("\\", "/").Equals(y.Replace("\\", "/")))).ToList();
            
            return fileStatusList;
        }

        private void GetAndShowCommitwindow(IEnumerable<FileStatus> statusList)
        {
            var window = EditorWindow.GetWindow<CommitWindow>("Commit");
            window.StatusList = statusList.ToList();
            window.OnCommit = OnCommit;
            window.Show();
        }

        private List<FileStatus> GetAddedFilesInFolders(List<FileStatus> fileStatusList)
        {
            var folders = fileStatusList.Where(x => x.Status == Status.Added && String.IsNullOrEmpty(x.Extension)).ToList();
            foreach (var folder in folders)
            {
                var files = new FileStatus[0];
                if (Directory.Exists(folder.FullPath))
                {
                    files =
                        Directory.GetFiles(folder.FullPath, "*.*", SearchOption.AllDirectories)
                            .Select(x => new GitFileStatus(x, Status.Added) as FileStatus)
                            .ToArray();
                    if (files.Any())
                    {
                        fileStatusList.AddRange(files);
                        fileStatusList.Remove(folder);
                    }
                }
            }
            return fileStatusList;
        }

        private void OnCommit(CommitWindow window)
        {
            try
            {
                UnstageAll();

                var files = window.StatusList.Where(x => x.Commit).Select(x => x.FullPath);
                Add(files);

                Commit(window.Message);
                Push();

                EditorUtility.DisplayDialog("Success", "Successfully commited changes!", "Ok");
            }
            finally
            {
                window.Close();
            }
        }

        private void Add(IEnumerable<string> files)
        {
            files = AddQuatationMarks(files.ToArray());
            var filesLine = String.Join(" ", files.ToArray());
            var cmd = GetCmd("add " + filesLine, new[]
            {
                "^(?<error>error: .+)$",
                "^(?<error>fatal: .+)$",
            });
            cmd.Run();
        }

        private void Commit(string message)
        {
            if (String.IsNullOrEmpty(message))
                message = @"--allow-empty-message -m """;
            else
                message = "-m " + AddQuatationMarks(message);
            var cmd = GetCmd(String.Format("commit {0}", message), new[]
            {
                "^(?<error>error: .+)$",

                @"^(?<skip>\s+\d+ files? changed,?(\s+\d+ insertions?[(][+][)])?,?(\s+\d+ deletions?[(][-][)])?)$",
                "^(?<skip>[[].+ .+[]] .+)$",
                @"^(?<skip>\s+(create|delete) mode .+)$",
                @"^(?<skip>\s+rename .+[{].+ => .+[}].*)$"
            });
            cmd.Run();
        }

        private void Push()
        {
            var cmd = GetCmd("push", new[]
            {
                "^(?<skip>To .+)$",
                "^(?<skip>hint: .+)$",
                @"^(?<skip>Counting objects: \d+, done[.])$",
                @"^(?<skip>Delta compression using up to \d+ threads?[.])$",
                "^(?<skip>Compressing objects: .+)$",
                "^(?<skip>Writing objects: .+)$",
                @"^(?<skip>Total \d+ [(]delta \d+[)], reused \d+ [(]delta \d+[)])$",
                "^(?<skip>remote: .+)$",
                @"^(?<skip>\s+.+\s+.+ -> .+)$",

                @"^(?<error>\s+!\s+[[]rejected[]]\s+.+ -> .+)$",
                "^(?<error>error: .+)$",
            });
            cmd.Run();
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
    }
}