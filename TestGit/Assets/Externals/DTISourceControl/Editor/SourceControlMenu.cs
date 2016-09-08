﻿using System;
using System.IO;
using DTI.SourceControl.Git;
using DTI.SourceControl.Svn;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
    public class SourceControlMenu
    {
        [MenuItem("DTI/Source Control/Options", false, 20)]
        public static void ShowOptionsWindow()
        {
            var manager = GetControlManager();
            manager.ShowOptionsWindow();
        }

        [MenuItem("DTI/Source Control/Update", false, 1)]
        public static void Update()
        {
            var manager = GetControlManager();
            manager.UpdateAll();
        }

        [MenuItem("DTI/Source Control/Commit All", false, 2)]
        public static void ShowCommitWindow()
        {
            var manager = GetControlManager();
            manager.ShowCommitWindowAll();
        }

        [MenuItem("Assets/Source Control/Commit")]
        public static void ShowCommitWindowSelected()
        {
            var manager = GetControlManager();
            manager.ShowCommitWindowSelected();
        }

        [MenuItem("DTI/Source Control/Choose Branch", false, 3)]
        public static void ShowChooseBranchWindow()
        {
            var manager = GetControlManager();
            manager.ShowChooseBranchWindow();
        }

        private static ISourceControlManager GetControlManager()
        {
            var dir = Path.GetDirectoryName(Application.dataPath);
            while (!String.IsNullOrEmpty(dir))
            {
                var path = Path.Combine(dir, ".svn");
                if (Directory.Exists(path))
                    return new SvnManager(dir);
                path = Path.Combine(dir, ".git");
                if (Directory.Exists(path))
                    return new GitManager(dir);
                dir = Path.GetDirectoryName(dir);
            }

            EditorUtility.DisplayDialog("No version control system",
                "No version control system was found. Don't touch these buttons!", "Ok");
            return null;
        }
    }
}