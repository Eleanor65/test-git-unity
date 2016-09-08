using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DTI.SourceControl.ConsoleTools;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Svn
{
    internal class SvnManager : SourceControlManagerBase
	{
        private const string OPERATIONUPDATE = "update";
        private const string OPERATIONCOMMIT = "commit";
        private const string OPTIONDEPTHEMPTY = "--depth empty";
        private const string OPTIONPARENTS = "--parents";
	    private readonly SvnTools _tools = new SvnTools();
        private readonly string _repositoryFolder;
        private readonly string _svn;

        public SvnManager(string directory)
        {
            _repositoryFolder = directory;
            _svn = EditorPrefs.GetString(_tools.PathKey);
        }

	    public override void ShowOptionsWindow()
        {
            var window = EditorWindow.GetWindow<OptionsWindow>("Svn Options");
	        window.Tools = _tools;
            window.Show();
        }

        public override void UpdateAll()
        {
            string projectPath = _repositoryFolder;
            Debug.Log("Updating current project");

            var cmd = new Cmd()
            {
                BaseDirectory = projectPath,
                Command = _svn,
                Args = String.Format("{0} --accept p", OPERATIONUPDATE),
                Patterns = new[]
                {
                    "^(?<skip>Updating '.*':)$",
                    "^(?<skip>At revision [0-9]+[.])$",
                    "^(?<skip>Updated to revision [0-9]+[.])$",
                    "^(?<skip>[ADU][ ]+.+)$",
                    "^(?<skip>Summary of conflicts:)$",
                    "^(?<skip>[]+Text conflicts: [0-9]+)$",
                    "^(?<skip>Fetching external item into '.+':)$",
                    @"^(?<skip>Updated external to revision \d+[.])$",
                    @"^(?<skip>External at revision \d+[.])$",

                    "^(?<out>C[ ]+.+)$",

                    "^(?<error>svn: E[0-9]+: .*)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            var outResult = cmd.Run();
            if (outResult.Count != 0)
            {
                ShowConflicts(outResult, OPERATIONUPDATE);
            }
            else
            {
                Debug.Log("The project was successfully updated!");
            }
        }

	    public override void ShowCommitWindowAll()
	    {
            var statusList = GetProjectStatus();
	        GetAndShowCommitWindow(statusList);
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
            GetAndShowCommitWindow(statusList);
        }

	    public override void ShowChooseBranchWindow()
	    {
	        EditorUtility.DisplayDialog("No branches",
	            "Your current project uses SVN as source conrol system.\n\nNo branches!", "Ok");
	    }

        private List<FileStatus> GetProjectStatus()
        {
            var fileStatusList = GetStatusForFiles(new[] {Application.dataPath}, false);
            fileStatusList = AddNUVCFiles(fileStatusList);
            fileStatusList = DeleteMissingFiles(fileStatusList);

            return fileStatusList;
        }

        private List<FileStatus> GetStatus(string[] paths)
        {
            var fileStatusList = GetStatusForFiles(paths, false);

            fileStatusList = AddAddedParentFolders(fileStatusList);
            fileStatusList = AddNUVCFolders(fileStatusList);
            fileStatusList = DeleteMissingFiles(fileStatusList);

            return fileStatusList;
        }

        private List<FileStatus> GetStatusForFiles(string[] paths, bool depthEmpty = true)
        {
            var args = "status";
            if (depthEmpty)
                args += " --depth empty";
            else
                paths = RemoveChildPaths(paths);
            paths = AddQuatationMarks(paths);
            args += " " + String.Join(" ", paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _svn,
                Args = args,
                Patterns = new[]
                {
                    "^(?<skip>Summary of conflicts:)$",
                    "^(?<skip>[ ]+Text conflicts: [0-9]+)$",

                    "^(?<out>[?!DAMC][ ]+.+)$",
                    "^(?<out>svn: warning: W155010: The node '.+' was not found.)$"
                }
            };
            var outResult = cmd.Run();
            var fileStatusList = new List<FileStatus>();
            if (outResult.Count == 0)
                return fileStatusList;

            fileStatusList = outResult.Select(x => new SvnFileStatus(x) as FileStatus).ToList();
            return fileStatusList;
        }

        private List<FileStatus> AddAddedParentFolders(List<FileStatus> list)
        {
            var added = list.Where(x => x.Status == Status.Added || x.Status == Status.NotUnderVC);
            if (!added.Any())
                return list;

            var folders = added.Select(x => Path.GetDirectoryName(x.FullPath)).Distinct().ToArray();
            var folderMetas = folders.Where(x => File.Exists(x + METAEXTENSION)).Select(x => x + METAEXTENSION);
            folders = folders.Concat(folderMetas).ToArray();

            var folderList = GetStatusForFiles(folders);
            var metas = folderList.Where(x => x.Extension.Equals(METAEXTENSION)).ToList();
            list = FileStatus.UpdateList(list, metas);

            var addedFolders = folderList.Where(x => x.Status == Status.Added);
            if (addedFolders.Any())
            {
                addedFolders = AddAddedParentFolders(addedFolders.ToList());
                list = FileStatus.UpdateList(list, addedFolders.ToList());
            }

            return list;
        }

        private List<FileStatus> AddNUVCFolders(List<FileStatus> list)
        {
            var notFoundPaths = list.Where(x => x.Status == Status.NotFound).Select(x=> x.FullPath).ToArray();
            if (!notFoundPaths.Any())
                return list;

            var added = Add(notFoundPaths, OPTIONPARENTS);
            var metasPaths = added.Where(x => File.Exists(x.FullPath + METAEXTENSION)).Select(x => x.FullPath + METAEXTENSION).ToArray();
            if (metasPaths.Any())
                list = FileStatus.UpdateList(list, GetStatusForFiles(metasPaths));
            list = FileStatus.UpdateList(list, added);

            return list;
        }

        private List<FileStatus> AddNUVCFiles(List<FileStatus> list)
        {
            var notUnderVC = list.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray();
            if (!notUnderVC.Any())
                return list;
            
            var added = Add(notUnderVC);
            if (added.Any())
                list = FileStatus.UpdateList(list, added);

            return list;
        }

        private List<FileStatus> DeleteMissingFiles(List<FileStatus> list)
        {
            var missing = list.Where(x => x.Status == Status.Missing).Select(x => x.FullPath).ToArray();
            if (missing.Length == 0)
                return list;

            Delete(missing);
            return GetStatusForFiles(list.Select(x => x.FullPath).ToArray());
        }

        private void GetAndShowCommitWindow(List<FileStatus> statusList)
        {
            var window = EditorWindow.GetWindow<CommitWindow>("Commit");
            window.StatusList = statusList;
            window.OnCommit = OnCommit;
            window.Show();
        }

        private void OnCommit(CommitWindow window)
        {
            var commitList = window.StatusList.Where(x => x.Commit);
            if (commitList.Any(x => x.Status == Status.Conflicted))
            {
                EditorUtility.DisplayDialog("Can't commit!",
                    String.Format(
                        "Conflicted files have been chosen for commit. Please, resolve coflicts before commit.\n\nConflicted files:\n{0}",
                        String.Join("\n",
                            commitList.Where(x => x.Status == Status.Conflicted)
                                .Select(x => x.RelativePath)
                                .ToArray())), "Ok");
            }
            else
            {
                Commit(commitList, window.Message ?? String.Empty);
                window.Close();
            }
        }

        private void Commit(IEnumerable<FileStatus> statusList, string messege)
        {
            var notUnderVC = statusList.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray();
            if (notUnderVC.Length != 0)
                Add(notUnderVC, OPTIONDEPTHEMPTY);
            CommitFiles(statusList.Select(x => x.FullPath).ToArray(), messege);
        }

        private List<FileStatus> Add(string[] paths, params string[] options)
        {
            var args = "add";
            var depthEmpy = false;
            foreach (var option in options)
            {
                args += " " + option;
                if (option.Equals(OPTIONDEPTHEMPTY))
                    depthEmpy = true;
            }
            if (!depthEmpy)
                paths = RemoveChildPaths(paths);

            paths = AddQuatationMarks(paths);
            args += " " + String.Join(" ", paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _svn,
                Args = args,
                Patterns = new[]
                {
                    @"^(?<out>[A]\s+([(]bin[)]\s+)?.+)$",

                    "^(?<error>svn: E.+)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            var outResult = cmd.Run();
            var fileStatusList = new List<FileStatus>();
            if (outResult.Count == 0)
                return fileStatusList;

            fileStatusList = outResult.Select(x => new SvnFileStatus(x) as FileStatus).ToList();
            return fileStatusList;
        }

        private void Delete(string[] paths)
        {
            Debug.Log("Deleting files: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _svn,
                Args = "delete " + String.Join(" ", correctedPaths),
                Patterns = new[]
                {
                    "^(?<skip>[D][ ]+.+)$",

                    "^(?<error>svn: E.+)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            cmd.Run();
        }

        private void CommitFiles(string[] paths, string message)
        {
            Debug.Log("Commiting: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _svn,
                Args = String.Format("{0} {1} -m \"{2}\" --depth empty", OPERATIONCOMMIT, String.Join(" ", correctedPaths), message),
                Patterns = new[]
                {
                    "^(?<skip>[ADU][ ]+.+)$",
                    "^(?<skip>Sending[ ]+.+)$",
                    "^(?<skip>Adding[ ]+.+)$",
                    "^(?<skip>Deleting[ ]+.+)$",
                    "^(?<skip>Transmitting file data [.]*done)$",
                    "^(?<skip>Committing transaction...)$",
                    "^(?<skip>Committed revision [0-9]+[.])$",

                    "^(?<out>C[ ]+.+)$",

                    "^(?<error>svn: E.+)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            var outResult = cmd.Run();
            if (outResult.Count != 0)
            {
                ShowConflicts(outResult, OPERATIONCOMMIT);
            }
            else
            {
                Debug.Log("Commit has ended successfully!");
            }
        }

        private void ShowConflicts(IEnumerable<string> outResult, string operation)
        {
            var regex = new Regex("^(C[ ]+)(?<path>[^ ].*)$");
            var conflicts = String.Join("\n", outResult.Select(x => regex.Match(x).Groups["path"].Value).ToArray());
            var message = String.Format("During {0} operation there were found {1} conflicts:\n\n{2}\n\nConflicts need to be resolved!",
                operation, outResult.Count(), conflicts);
            EditorUtility.DisplayDialog("Conflicts were found!", message, "Ok");
            Debug.LogError(message);
        }

        private string[] RemoveChildPaths(string[] paths)
        {
            var onlyNeeded = paths.Where(x => paths.All(y => !x.StartsWith(y) || x.Equals(y + METAEXTENSION) || x == y)).ToArray();

            return onlyNeeded;
        }
	}
}
