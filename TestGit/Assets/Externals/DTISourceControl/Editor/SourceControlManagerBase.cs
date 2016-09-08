using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
    public abstract class SourceControlManagerBase : ISourceControlManager
    {
        protected const string METAEXTENSION = ".meta";

        public abstract void ShowOptionsWindow();

        public abstract void UpdateAll();

        public abstract void ShowCommitWindowAll();

        public abstract void ShowCommitWindowSelected();

        public abstract void ShowChooseBranchWindow();

        protected string AddQuatationMarks(string line)
        {
            var newLine = String.Copy(line);
            if (newLine.Contains(" "))
                newLine = '"' + newLine + '"';
            return newLine;
        }

        protected string[] AddQuatationMarks(string[] lines)
        {
            var newLines = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                newLines[i] = AddQuatationMarks(lines[i]);
            return newLines;
        }

        protected static string[] GetSelectedAssetPaths()
        {
            var paths = Selection.assetGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
            var assets = paths.Select(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x)).ToList();
            var scenes = assets.Where(x => x is SceneAsset);

            var folders = assets.Where(x => x is DefaultAsset);
            if (folders.Count() != 0)
            {
                var assetsInFoldersGUIDs =
                    AssetDatabase.FindAssets("", folders.Select(x => AssetDatabase.GetAssetPath(x)).ToArray());
                var assetsInFoldersPaths = assetsInFoldersGUIDs.Select(x => AssetDatabase.GUIDToAssetPath(x));
                assets.AddRange(assetsInFoldersPaths.Select(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x)));
                folders = assets.Where(x => x is DefaultAsset);
                assets = assets.Where(x => !(x is DefaultAsset)).ToList();
            }

            paths = EditorUtility.CollectDependencies(assets.ToArray()).Select(x => AssetDatabase.GetAssetPath(x)).ToList();
            paths = paths.Where(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x) != null).ToList();

            paths.AddRange(folders.Select(x => AssetDatabase.GetAssetPath(x)));
            paths.AddRange(scenes.Select(x => AssetDatabase.GetAssetPath(x)));
            paths = paths.Distinct().ToList();
            var metas = paths.Where(x => File.Exists(x + METAEXTENSION)).Select(x => x + METAEXTENSION).ToList();
            paths.AddRange(metas);
            paths = paths.Select(x => Path.Combine(Path.GetDirectoryName(Application.dataPath), x)).ToList();

            return paths.ToArray();
        }
    }
}