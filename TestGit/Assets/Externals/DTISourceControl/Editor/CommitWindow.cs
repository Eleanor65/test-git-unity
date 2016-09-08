using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DTI.SourceControl.Validation;
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
            }
        }

        public string Message { get { return _message; } }

        private void OnGUI()
        {
            if (_statusList == null)
            {
                EditorGUILayout.LabelField("_statusList = null");
                return;
            }

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
                EditorGUILayout.Space();
            }
        }

        private void ShowButtons()
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_statusList.All(x => !(x.Commit)));
            if (GUILayout.Button("Commit", GUILayout.Width(BUTTONWIDTH)))
            {
                if (String.IsNullOrEmpty(Message))
                {
                    if (!EditorUtility.DisplayDialog("Message is empty!",
                        "Message is empty. Are you sure you want to commit without a message?", "Yes", "No"))
                        return;
                }

                if (!IsValidCommit())
                    return;

                if (OnCommit != null)
                    OnCommit(this);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Cancel", GUILayout.Width(BUTTONWIDTH)))
                this.Close();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(String.Format("{0} files selected, {1} total.", _statusList.Count(x => x.Commit), _statusList.Count));
            GUILayout.EndHorizontal();
        }

        private bool IsValidCommit()
        {
            var start = DateTime.Now;
            var type = typeof (ICommitValidator);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(s => s.GetTypes())
                                 .Where(p => p != type && type.IsAssignableFrom(p));
            var validators = types.Select(x => (ICommitValidator)Activator.CreateInstance(x));
            var duration = DateTime.Now.Subtract(start);
            Debug.Log("Durations of finding all validators: " + duration);

            var isValid = true;
            var errorMessages = new List<string>();
            var files = _statusList.Where(x => x.Commit).Select(x => x.RelativePath);
            foreach (var validator in validators)
            {
                IEnumerable<string> errors;
                if (!validator.IsValid(files, out errors))
                {
                    isValid = false;
                    errorMessages.AddRange(errors);
                }
            }

            if (!isValid)
            {
                var message = String.Empty;
                if (errorMessages.Any())
                    message = String.Join("\n", errorMessages.ToArray()) + "\n\n";
                message += "You need to resolve all validation errors before commit.";
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Validation error", message, "Ok");
            }

            return isValid;
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
            GUILayout.BeginVertical(GUILayout.Width(300));
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
            var width = this.position.width - 763;
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
            _nodes = _statusList.Select(x => new HierarchyNode(x)).ToList();

            if (_statusList.All(x => String.IsNullOrEmpty(x.Extension)))
                return;

            foreach (var node in _nodes)
            {
                var parent = _nodes.SingleOrDefault(y => y.Value.FullPath.Equals(Path.GetDirectoryName(node.Value.FullPath)));
                if (parent != null)
                    node.Parent = parent;
            }
        }
    }
}