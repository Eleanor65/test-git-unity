using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
    public class CommitWindow : EditorWindow
    {
        private const int TOGGLEWIDTH = 15;
        private const int BUTTONWIDTH = 70;
        private List<FileStatus> _statusList;
        private List<HierarchyNode> _nodes;
        //private HierarchyNode _tree;

        private Vector2 _scrollPos;
        private string _message;
        private bool _commitAll;
        
        public delegate void OnCommitDelegate(CommitWindow window);
        public OnCommitDelegate OnCommit;

        private bool CommitAll
        {
            get { return _commitAll; }
            set
            {
                if (_commitAll != value)
                {
                    _commitAll = value;
                    _statusList = _statusList.Select(x =>
                    {
                        if (x.Commit != value)
                            x.Commit = value;
                        return x;
                    }).ToList();
                }
            }
        }

        public List<FileStatus> StatusList
        {
            get { return _statusList; }
            set
            {
                _statusList = value;
                _statusList = _statusList.OrderBy(x => x.FullPath).ToList();
                SetNodes();
                //SetTree();
                //SortNode(_tree);
            }
        }

        public string Message { get { return _message; } }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            GUILayout.Label("Message:");
            _message = EditorGUILayout.TextArea(_message);
            ShowFiles();
            EditorGUILayout.EndScrollView();
            ShowButtons();
        }

        private void ShowFiles()
        {
            if (_statusList.Count == 0)
            {
                EditorGUILayout.LabelField("Nothing to commit.");
            }
            else
            {
                GUILayout.BeginHorizontal();
                ShowCommitValues();
                ShowStatus();
                ShowName();
                ShowExtension();
                ShowPath();
                GUILayout.EndHorizontal();
                //ShowNode(_tree);
                EditorGUILayout.Space();
                //CommitAll = EditorGUILayout.ToggleLeft("Select All", CommitAll);
            }
        }

        //private void ShowNode(HierarchyNode node)
        //{
        //    GUILayout.BeginHorizontal();
        //    if (node.Committable)
        //        node.Commit = EditorGUILayout.Toggle(node.Commit, GUILayout.Width(TOGGLEWIDTH));
        //    else 
        //        GUILayout.Space(23);
        //    var guiContent = new GUIContent(node.Value.Name, node.Value.RelativePath);
        //    if (node.Committable)
        //        guiContent.text += "     " + node.Value.Status;
        //    if (node.Children == null)
        //    {
        //        guiContent.text = "   " + guiContent.text;
        //        EditorGUILayout.LabelField(guiContent);
        //    }
        //    else
        //    {
        //        //node.Foldout = EditorGUILayout.Foldout(node.Foldout, guiContent);
        //        var rect = GUILayoutUtility.GetRect(guiContent, EditorStyles.foldout, GUILayout.MaxWidth(10));
        //        node.Foldout = EditorGUI.Foldout(rect, node.Foldout, guiContent, EditorStyles.foldout);

        //        if (node.Foldout)
        //        {
        //            GUILayout.BeginVertical();
        //            GUILayout.Space(18);
        //            foreach (var childNode in node.Children)
        //            {
        //                ShowNode(childNode);
        //            }
        //            GUILayout.EndVertical();
        //        }
        //    }
        //    GUILayout.EndHorizontal();
        //}

        private void ShowButtons()
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_statusList.All(x => !(x.Commit)));
            if (GUILayout.Button("Commit", GUILayout.Width(BUTTONWIDTH)))
            {
                if (OnCommit != null)
                    OnCommit(this);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Cancel", GUILayout.Width(BUTTONWIDTH)))
                this.Close();
            GUILayout.EndHorizontal();
        }

        private void ShowCommitValues()
        {
            GUILayout.BeginVertical(GUILayout.Width(TOGGLEWIDTH));
            GUILayout.Space(3);
            CommitAll = EditorGUILayout.Toggle(CommitAll);
            foreach (var node in _nodes)
            {
                node.Commit = EditorGUILayout.Toggle(node.Commit);
            }
            GUILayout.EndVertical();
        }

        private void ShowStatus()
        {
            GUILayout.BeginVertical(GUILayout.Width(100));
            if (GUILayout.Button("Status"))
                _statusList = _statusList.OrderBy(x => x.Status).ToList();
            foreach (FileStatus status in _statusList)
                EditorGUILayout.LabelField(status.Status.ToString());
            GUILayout.EndVertical();
        }

        private void ShowName()
        {
            GUILayout.BeginVertical(GUILayout.Width(100));
            if (GUILayout.Button("Name"))
                _statusList = _statusList.OrderBy(x => x.Name).ToList();
            foreach (FileStatus status in _statusList)
                EditorGUILayout.LabelField(status.Name);
            GUILayout.EndVertical();
        }

        private void ShowExtension()
        {
            GUILayout.BeginVertical(GUILayout.Width(100));
            if (GUILayout.Button("Extension"))
                _statusList = _statusList.OrderBy(x => x.Extension).ToList();
            foreach (FileStatus status in _statusList)
                EditorGUILayout.LabelField(status.Extension);
            GUILayout.EndVertical();
        }

        private void ShowPath()
        {
            var width = this.position.width - 653;
            if (width < 100)
                width = 100;
            GUILayout.BeginVertical(GUILayout.Width(width));
            if (GUILayout.Button("Path"))
                _statusList = _statusList.OrderBy(x => x.RelativePath).ToList();
            foreach (FileStatus status in _statusList)
                EditorGUILayout.LabelField(status.RelativePath);
            GUILayout.EndVertical();
        }

        private void SetNodes()
        {
            _nodes =
                _statusList.Where(x => _statusList.All(y => !(y.FullPath.Equals(Path.GetDirectoryName(x.FullPath)))))
                    .Select(x => new HierarchyNode(x))
                    .ToList();

            _nodes = SetChildren(_nodes);
        }

        private List<HierarchyNode> SetChildren(List<HierarchyNode> nodes)
        {
            foreach (HierarchyNode node in nodes)
            {
                foreach (var fileStatus in _statusList)
                {
                    if (IsParent(node.Value, fileStatus))
                    {
                        var newNode = new HierarchyNode(fileStatus, node);
                        _nodes.Add(newNode);
                    }
                }
                if (node.Children != null)
                    node.Children = SetChildren(node.Children);
            }

            return nodes;
        }

        //private void SetTree()
        //{
        //    _tree = new HierarchyNode(new FileStatus(Application.dataPath))
        //    {
        //        Committable = false
        //    };

        //    foreach (var node in _nodes)
        //    {
        //        var parentsPaths = new List<String>();
        //        var parentPath = Path.GetDirectoryName(node.Value.FullPath);
        //        while (!parentPath.Equals(_tree.Value.FullPath.Replace('/', '\\')))
        //        {
        //            parentsPaths.Add(parentPath);
        //            parentPath = Path.GetDirectoryName(parentPath);
        //        }

        //        var currParent = _tree;
        //        for (int i = parentsPaths.Count - 1; i >= 0; i--)
        //        {
        //            HierarchyNode child;
        //            if (currParent.Children != null &&
        //                currParent.Children.Any(x => x.Value.FullPath.Replace('/', '\\').Equals(parentsPaths[i])))
        //            {
        //                child =
        //                    currParent.Children.First(x => x.Value.FullPath.Replace('/', '\\').Equals(parentsPaths[i]));
        //            }
        //            else
        //            {
        //                child = new HierarchyNode(new FileStatus(parentsPaths[i]), currParent, false);
        //            }
        //            currParent = child;
        //        }
        //        node.Parent = currParent;
        //    }
        //}

        private void SortNode(HierarchyNode node)
        {
            if (node.Children == null)
                return;
            node.Children = node.Children.OrderBy(x => x.Children != null).ThenBy(x => x.Value.Name).ToList();
            foreach (var child in node.Children)
            {
                SortNode(child);
            }
        }

        private bool IsParent(FileStatus parent, FileStatus child)
        {
            return parent.FullPath.Equals(Path.GetDirectoryName(child.FullPath));
        }
    }
}