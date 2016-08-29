﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DTI.SourceControl.ConsoleTools;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Svn
{
	internal class SvnManager : ISourceControlManager
	{
        private const string OperationUpdate = "update";
        private const string OperationCommit = "commit";
	    private readonly SvnTools _tools = new SvnTools();

	    public void ShowOptionsWindow()
        {
            var window = EditorWindow.GetWindow<OptionsWindow>("Svn Options");
	        window.Tools = _tools;
            window.Show();
        }

        public void UpdateAll()
        {
            string projectPath = GetProjectPath();
            projectPath = AddQuatationMarks(projectPath);
            Debug.Log("Updating current project");

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _tools.Executable,
                Args = String.Format("{0} {1} --accept p", OperationUpdate, projectPath),
                Patterns = new[]
                {
                    "^(?<skip>Updating '.*':)$",
                    "^(?<skip>At revision [0-9]+[.])$",
                    "^(?<skip>Updated to revision [0-9]+[.])$",
                    "^(?<skip>[ADU][ ]+.+)$",
                    "^(?<skip>Summary of conflicts:)$",
                    "^(?<skip>[]+Text conflicts: [0-9]+)$",

                    "^(?<out>C[ ]+.+)$",

                    "^(?<error>svn: E[0-9]+: .*)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            var outResult = cmd.Run();
            if (outResult.Count != 0)
            {
                ShowConflicts(outResult, OperationUpdate);
            }
            else
            {
                Debug.Log("The project was successfully updated!");
            }
        }

	    public void ShowCommitWindow()
	    {
            var selected = GetSelectedAssetPaths();
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Error",
                    "No assets were chosen to commit. Choose some asset/assets to commit.", "OK");
                return;
            }

            var statusList = GetStatus(selected);
            var window = EditorWindow.GetWindow<CommitWindow>("Commit");
            window.StatusList = statusList;
	        window.OnCommit = OnCommit;
            window.Show();
	    }

	    public void ShowChooseBranchWindow()
	    {
	        EditorUtility.DisplayDialog("No branches",
	            "Your current project uses SVN as source conrol system.\n\nNo branches!", "Ok");
	    }

        private List<FileStatus> GetStatus(string[] paths)
        {
            var fileStatusList = GetStatusForFiles(paths);

            if (fileStatusList.Any(x => x.Status == Status.Added || x.Status == Status.NotUnderVC))
                fileStatusList = AddAddedFolders(fileStatusList);

            if (fileStatusList.Any(x => x.Status == Status.NotFound))
                fileStatusList = AddMissingFolders(fileStatusList);

            return fileStatusList;
        }

        private List<FileStatus> GetStatusForFiles(string[] paths)
        {
            Debug.Log("Getting status for: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _tools.Executable,
                Args = "status " + String.Join(" ", correctedPaths) + " --depth empty",
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

        private List<FileStatus> AddAddedFolders(List<FileStatus> list)
        {
            var added = list.Where(x => x.Status == Status.Added || x.Status == Status.NotUnderVC);
            var folders = added.Select(x => Path.GetDirectoryName(x.FullPath)).Distinct().ToArray();
            var folderMetas = folders.Where(x => File.Exists(x + ".meta")).Select(x => x + ".meta");
            folders = folders.Concat(folderMetas).ToArray();

            var folderList = GetStatusForFiles(folders);
            if (folderList.Any(x => x.Status == Status.Added))
            {
                var addedFolders = folderList.Where(x => x.Status == Status.Added);
                addedFolders = AddAddedFolders(addedFolders.ToList());
                list = FileStatus.UpdateList(list, addedFolders.ToList());
            }

            return list;
        }

        private List<FileStatus> AddMissingFolders(List<FileStatus> list)
        {
            var notFound = list.Where(x => x.Status == Status.NotFound);
            var folders = notFound.Select(x => Path.GetDirectoryName(x.FullPath)).Distinct().ToArray();
            var folderMetas = folders.Where(x => File.Exists(x + ".meta")).Select(x => x + ".meta");
            folders = folders.Concat(folderMetas).ToArray();

            var folderList = GetStatusForFiles(folders);
            if (folderList.Any(x => x.Status == Status.NotUnderVC))
            {
                var foldersNUVC = folderList.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray();
                Add(foldersNUVC);
                var folderListAdded = GetStatusForFiles(foldersNUVC);
                folderList = FileStatus.UpdateList(folderList, folderListAdded);
            }
            if (folderList.Any(x => x.Status == Status.NotFound))
            {
                folderList = AddMissingFolders(folderList);
            }
            list = FileStatus.UpdateList(list, folderList);

            notFound = GetStatusForFiles(notFound.Select(x => x.FullPath).ToArray());
            if (notFound.Any(x => x.Status == Status.NotUnderVC))
            {
                var notUnderVC = notFound.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray();
                Add(notUnderVC);
                var added = GetStatusForFiles(notUnderVC);
                notFound = FileStatus.UpdateList(notFound.ToList(), added);
            }
            list = FileStatus.UpdateList(list, notFound.ToList());

            return list;
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
                if (String.IsNullOrEmpty(window.Message))
                {
                    if (!EditorUtility.DisplayDialog("Message is empty!",
                        "Message is empty. Are you sure you want to commit without a message?", "Yes", "No"))
                        return;
                }
                this.Commit(commitList, window.Message ?? String.Empty);
                window.Close();
            }
        }

        private void Commit(IEnumerable<FileStatus> statusList, string messege)
        {
            if (statusList.Any(x => x.Status == Status.NotUnderVC))
                Add(statusList.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray());
            if (statusList.Any(x => x.Status == Status.Missing))
                Delete(statusList.Where(x => x.Status == Status.Missing).Select(x => x.FullPath).ToArray());
            CommitFiles(statusList.Select(x => x.FullPath).ToArray(), messege);
        }

        private void Add(string[] paths)
        {
            Debug.Log("Adding files: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _tools.Executable,
                Args = "add " + String.Join(" ", correctedPaths) + " --depth empty",
                Patterns = new[]
                {
                    "^(?<skip>[A][ ]+.+)$",

                    "^(?<error>svn: E.+)$",
                    "^(?<error>svn: warning: W.+)$",
                }
            };
            cmd.Run();
        }

        private void Delete(string[] paths)
        {
            Debug.Log("Deleting files: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = _tools.SvnDirectory,
                Command = _tools.Executable,
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
                Command = _tools.Executable,
                Args = String.Format("{0} {1} -m \"{2}\" --depth empty", OperationCommit, String.Join(" ", correctedPaths), message),
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
                ShowConflicts(outResult, OperationCommit);
            }
            else
            {
                Debug.Log("Commit has ended successfully!");
            }
        }

	    private string GetProjectPath()
	    {
            var dir = Path.GetDirectoryName(Application.dataPath);
	        while (!String.IsNullOrEmpty(dir))
	        {
	            var path = Path.Combine(dir, ".svn");
	            if (Directory.Exists(path))
	                return Path.GetDirectoryName(path);
	            dir = Path.GetDirectoryName(dir);
	        }
	        return null;
	    }

        private string AddQuatationMarks(string line)
        {
            var newLine = String.Copy(line);
            if (newLine.Contains(" "))
                newLine = '"' + newLine + '"';
            return newLine;
        }

        private string[] AddQuatationMarks(string[] lines)
        {
            var newLines = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                newLines[i] = AddQuatationMarks(lines[i]);
            return newLines;
        }

        private static string[] GetSelectedAssetPaths()
        {
            var paths = Selection.assetGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
            var assets = paths.Select(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x)).ToList();

            var folders = assets.Where(x => x is DefaultAsset);
            if (folders.Count() != 0)
            {
                var assetsInFoldersGUIDs =
                    AssetDatabase.FindAssets("", folders.Select(x => AssetDatabase.GetAssetPath(x)).ToArray());
                var assetsInFoldersPaths = assetsInFoldersGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x));
                assets.AddRange(assetsInFoldersPaths.Select(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x)));
                folders = assets.Where(x => x is DefaultAsset);
            }
            assets = assets.Where(x => !(x is DefaultAsset)).ToList();

            paths = EditorUtility.CollectDependencies(assets.ToArray()).Select(x => AssetDatabase.GetAssetPath(x)).ToList();
            paths = paths.Where(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x) != null).ToList();

            paths.AddRange(folders.Select(x => AssetDatabase.GetAssetPath(x)));
            paths = paths.Distinct().ToList();
            var metas = paths.Where(x => File.Exists(x + ".meta")).Select(x => x + ".meta").ToList();
            paths.AddRange(metas);
            paths = paths.Select(x => Path.Combine(Path.GetDirectoryName(Application.dataPath), x)).ToList();

            return paths.ToArray();
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
	}
}
