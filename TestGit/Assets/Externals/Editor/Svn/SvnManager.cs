﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DTI.SourceControl.ConsoleTools;
using DTI.SourceControl.Svn;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
	internal class SvnManager
	{
        private const String OperationUpdate = "update";
	    private const String OperationCommit = "commit";

	    public void UpdateAll(string projectPath)
		{
		    projectPath = AddQuatationMarks(projectPath);
			Debug.Log("Updating current project");
		    
		    var cmd = new Cmd()
		    {
                BaseDirectory = EditorPrefs.GetString(SvnTools.PATHKEY),
                Command = SvnTools.Svn,
                Args = String.Format("{0} {1} --accept p", OperationUpdate, projectPath),
                Patterns = new []
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

        public List<FileStatus> GetStatus(String[] paths)
        {
            var fileStatusList = GetStatusForFiles(paths);

            if (fileStatusList.Any(x => x.Status == Status.Added || x.Status == Status.NotUnderVC))
                fileStatusList = AddAddedFolders(fileStatusList);

            if (fileStatusList.Any(x => x.Status == Status.NotFound))
                fileStatusList = AddMissingFolders(fileStatusList);

            return fileStatusList;
        }

	    public List<FileStatus> GetStatusForFiles(String[] paths)
	    {
            Debug.Log("Getting status for: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = SvnTools.SvnDirectory,
                Command = SvnTools.Svn,
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

            fileStatusList = outResult.Select(x => new FileStatus(x)).ToList();
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
                FileStatus.UpdateList(list, addedFolders.ToList());
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
                FileStatus.UpdateList(folderList, folderListAdded);
	        }
	        if (folderList.Any(x => x.Status == Status.NotFound))
	        {
	            folderList = AddMissingFolders(folderList);
	        }
            FileStatus.UpdateList(list, folderList);

	        notFound = GetStatusForFiles(notFound.Select(x => x.FullPath).ToArray());
	        if (notFound.Any(x => x.Status == Status.NotUnderVC))
	        {
	            var notUnderVC = notFound.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray();
                Add(notUnderVC);
	            var added = GetStatusForFiles(notUnderVC);
                FileStatus.UpdateList(notFound.ToList(), added);
	        }
	        FileStatus.UpdateList(list, notFound.ToList());

	        return list;
	    }

	    public void AddDeleteCommit(IEnumerable<FileStatus> statusList, String messege)
	    {
	        if (statusList.Any(x => x.Status == Status.NotUnderVC))
	            Add(statusList.Where(x => x.Status == Status.NotUnderVC).Select(x => x.FullPath).ToArray());
	        if (statusList.Any(x => x.Status == Status.Missing))
	            Delete(statusList.Where(x => x.Status == Status.Missing).Select(x => x.FullPath).ToArray());
            Commit(statusList.Select(x => x.FullPath).ToArray(), messege);
	    }

        public void Add(String[] paths)
	    {
	        Debug.Log("Adding files: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = SvnTools.SvnDirectory,
                Command = SvnTools.Svn,
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

        public void Delete(String[] paths)
	    {
            Debug.Log("Deleting files: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

            var cmd = new Cmd()
            {
                BaseDirectory = SvnTools.SvnDirectory,
                Command = SvnTools.Svn,
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

	    public void Commit(String[] paths, String message)
	    {
	        Debug.Log("Commiting: " + String.Join("\n", paths));
            var correctedPaths = AddQuatationMarks(paths);

	        var cmd = new Cmd()
	        {
                BaseDirectory = SvnTools.SvnDirectory,
	            Command = SvnTools.Svn,
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

	    private String AddQuatationMarks(String line)
	    {
	        var newLine = String.Copy(line);
            if (newLine.Contains(" "))
                newLine = '"' + newLine + '"';
            return newLine;
	    }

	    private String[] AddQuatationMarks(String[] lines)
	    {
	        var newLines = new string[lines.Length];
	        for (int i = 0; i < lines.Length; i++)
                newLines[i] = AddQuatationMarks(lines[i]);
            return newLines;
	    }

	    private void ShowConflicts(IEnumerable<String> outResult, String operation)
	    {
            var regex = new Regex("^(C[ ]+)(?<path>[^ ].*)$");
            var conflicts = String.Join("\n", outResult.Select(x => regex.Match(x).Groups["path"].Value).ToArray());
            EditorUtility.DisplayDialog("Conflicts were found!",
                String.Format(
                    "During {0} operation there were found {1} conflicts:\n\n{2}\n\nЗовите программиста, мы все умрем!",
                    operation, outResult.Count(), conflicts),
                "Ok");
            Debug.LogError(String.Format(
                "During {0} operation there were found {1} conflicts:\n{2}\nЗовите программиста, мы все умрем!",
                operation, outResult.Count(), conflicts));
	    }
	}
}
