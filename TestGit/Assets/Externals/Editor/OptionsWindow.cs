using System;
using System.IO;
using DTI.SourceControl.Git;
using DTI.SourceControl.Svn;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace DTI.SourceControl
{
    public class OptionsWindow : EditorWindow
    {
        private const int BUTTONWIDTH = 70;

        private String _path;
        private String _pathKey;
        private String _login;
        private String _loginKey;
        private String _password;
        private String _passwordKey;
        private String _executable;
        private VCSType _versionControlSystem;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Path to " + _executable);
            GUILayout.BeginHorizontal();
            _path = EditorGUILayout.TextField(_path);
            if (GUILayout.Button("Choose", GUILayout.Width(BUTTONWIDTH)))
            {
                _path = EditorUtility.OpenFilePanel("Choose folder with " + _executable, "", "");
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Login");
            _login = EditorGUILayout.TextField(_login);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Password");
            _password = EditorGUILayout.TextField(_password);
            EditorGUILayout.Space();

            if (GUILayout.Button("Save", GUILayout.Width(BUTTONWIDTH)))
            {
                var saved = SaveOptions();
                if (saved) 
                    this.Close();
            }
        }

        public void Show(VCSType type)
        {
            _versionControlSystem = type;
            SetKeys();
            LoadOptions();
            this.Show();
        }

        public void SetKeys()
        {
            switch (_versionControlSystem)
            {
                case VCSType.Svn:
                    _pathKey = SvnTools.PATHKEY;
                    _loginKey = SvnTools.LOGINKEY;
                    _passwordKey = SvnTools.PASSWORDKEY;
                    _executable = SvnTools.Svn;
                    break;
                case VCSType.Git:
                    _pathKey = GitTools.PATHKEY;
                    _loginKey = GitTools.LOGINKEY;
                    _passwordKey = GitTools.PASSWORDKEY;
                    _executable = GitTools.Git;
                    break;
            }
        }

        private void LoadOptions()
        {
            _path = EditorPrefs.GetString(_pathKey);
            _login = EditorPrefs.GetString(_loginKey);
            _password = EditorPrefs.GetString(_passwordKey);
        }

        private bool SaveOptions()
        {
            if (String.IsNullOrEmpty(_path) || !File.Exists(_path))
            {
                EditorUtility.DisplayDialog("Invalid path!", "This path is invalid! Please, choose correct path", "OK");
                return false;
            }

            EditorPrefs.SetString(_pathKey, _path);
            if (String.IsNullOrEmpty(_login))
                EditorPrefs.DeleteKey(_loginKey);
            else
                EditorPrefs.SetString(_loginKey, _login);
            if (String.IsNullOrEmpty(_password))
                EditorPrefs.DeleteKey(_passwordKey);
            else
                EditorPrefs.SetString(_passwordKey, _password);

            EditorUtility.DisplayDialog("Success!", "Options were successfully saved.", "OK");
            return true;
        }
    }
}