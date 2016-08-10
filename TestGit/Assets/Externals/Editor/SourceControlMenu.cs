using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DTI.SourceControl.Svn;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DTI.SourceControl
{
    public class SourceControlMenu
    {
        [MenuItem("DTI/Source Control/Svn/Options")]
        public static void ShowSvnOptionsWindow()
        {
            //var window = EditorWindow.GetWindow<SvnOptionsWindow>("Svn options");
            //window.LoadOptions();
            //window.Show();

            var win = EditorWindow.GetWindow<OptionsWindow>("Svn Options");
            win.Show(VCSType.Svn);
        }

        [MenuItem("DTI/Source Control/Svn/Update")]
        public static void SvnUpdate()
        {
            var manager = new SvnManager();
            var path = Path.GetDirectoryName(Application.dataPath);
            manager.UpdateAll(path);
        }

        [MenuItem("DTI/Source Control/Svn/Commit")]
        public static void SvnCommit()
        {
            var selected = GetSelectedAssetPaths();
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Error",
                    "No assets were chosen to commit. Choose some asset/assets to commit.", "OK");
                return;
            }

            var statusList = GetStatusSvn(selected);
            var window = EditorWindow.GetWindow<SvnCommitWindow>("Commit");
            window.StatusList = statusList;
            window.Show();
        }

        [MenuItem("DTI/Source Control/Git/Options")]
        public static void ShowGitOptionsWindow()
        {
            var window = EditorWindow.GetWindow<OptionsWindow>("Git Options");
            window.Show(VCSType.Git);
        }

        [MenuItem("DTI/Source Control/Git/Choose Branch")]
        public static void ShowGitBranchesWindow()
        {
            var window = EditorWindow.GetWindow<BranchesWindow>("Choose Branch");
            window.Show();
        }

        private static List<FileStatus> GetStatusSvn(String[] paths)
        {
            var manager = new SvnManager();
            return manager.GetStatus(paths);
        }

        private static String[] GetSelectedAssetPaths()
        {
            var paths = Selection.assetGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
            var assets = paths.Select(x => AssetDatabase.LoadAssetAtPath<Object>(x)).ToList();

            var folders = assets.Where(x => x is DefaultAsset);
            if (folders.Count() != 0)
            {
                //assets = assets.Where(x => !(x is DefaultAsset)).ToList();
                var assetsInFoldersGUIDs =
                    AssetDatabase.FindAssets("", folders.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()).ToList();
                var assetsInFoldersPaths = assetsInFoldersGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
                assets.AddRange(assetsInFoldersPaths.Select(x => AssetDatabase.LoadAssetAtPath<Object>(x)));
                folders = assets.Where(x => x is DefaultAsset);
            }
            assets = assets.Where(x => !(x is DefaultAsset)).ToList();

            //folders = GetSubFolders(folders.ToList());
            
            var log = String.Join("\n", assets.Select(x => String.Format("{0} {1}", x.name, x.GetType())).ToArray());
            Debug.Log(log);
            
            paths = EditorUtility.CollectDependencies(assets.ToArray()).Select(x => AssetDatabase.GetAssetPath(x)).ToList();
            paths = paths.Where(x => AssetDatabase.LoadAssetAtPath<Object>(x) != null).ToList();

            paths.AddRange(folders.Select(x => AssetDatabase.GetAssetPath(x)));
            paths = paths.Distinct().ToList();
            var meta = paths.Where(x => File.Exists(x + ".meta")).Select(x => x + ".meta").ToList();
            paths.AddRange(meta);
            paths = paths.Select(x => Path.Combine(Path.GetDirectoryName(Application.dataPath), x)).ToList();

            //log = String.Join("\n", paths.ToArray());
            //Debug.Log(log);

            return paths.ToArray();
        }

        private static List<Object> GetSubFolders(List<Object> folders)
        {
            var folderPaths = folders.Select(x => AssetDatabase.GetAssetPath(x));
            var subfoldersPaths = new string[0];
            foreach (var path in folderPaths)
            {
                subfoldersPaths = subfoldersPaths.Concat(AssetDatabase.GetSubFolders(path)).ToArray();
            }
            var subfolders = subfoldersPaths.Select(x => AssetDatabase.LoadAssetAtPath<Object>(x)).ToList();
            if (subfolders.Count != 0)
                subfolders = GetSubFolders(subfolders);
            folders.AddRange(subfolders);

            return folders;
        }
    }
}