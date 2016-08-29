using System;
using System.Collections.Generic;
using System.Linq;
using DTI.SourceControl.Git;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
    public class BranchesWindow : EditorWindow
    {
        private IEnumerable<Branch> _branches;
        private IEnumerable<Branch> _searchBranches;
        private Branch _currentBranch;
        private Branch _selectedBranch;
        private String _searchLine;
        private Vector2 scrollPos;

        public delegate void OnChooseBranchDelegate(BranchesWindow window);
        public OnChooseBranchDelegate OnChooseBranch;

        public IEnumerable<Branch> Branches
        {
            set
            {
                if (_branches != value)
                {
                    _branches = value;
                    InitializeBranches();
                }
            }
        }

        public Branch CurrentBranch { get { return _currentBranch; } }
        public Branch SelectedBranch { get { return _selectedBranch; } }

        private String SearchLine
        {
            get { return _searchLine; }
            set
            {
                if (_searchLine != value)
                {
                    _searchLine = value;
                    _searchBranches = _branches.Where(x => x.Name.StartsWith(_searchLine));
                }
            }
        }
        
        private void OnGUI()
        {
            ShowCurrentBranch();
            ShowSearchAndResults();
            ShowChooseButton();
        }

        private void InitializeBranches()
        {
            _currentBranch = _branches.Single(x => x.Current);
            
            _searchBranches = _branches;
            _selectedBranch = _searchBranches.First();
        }

        private void ShowCurrentBranch()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current branch:", GUILayout.Width(110));
            GUILayout.Button(_currentBranch.Name, GUILayout.Width(150));
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void ShowSearchAndResults()
        {
            SearchLine = EditorGUILayout.TextField("Search", SearchLine);

            EditorGUILayout.Space();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (var branch in _searchBranches)
            {
                if (GUILayout.Button(branch.Name, GUILayout.Width(150)))
                    _selectedBranch = branch;
            }
            GUILayout.EndScrollView();
            EditorGUILayout.Space();
        }

        private void ShowChooseButton()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Choose branch"), new GUIStyle() { fontStyle = FontStyle.Bold }, GUILayout.Width(110));
            if (GUILayout.Button(_selectedBranch.Name, GUILayout.Width(150)))
            {
                OnChooseBranch(this);
            }
            GUILayout.EndHorizontal();
        }
    }
}