using System;
using System.IO;
using UnityEditor;
using UnityEngine;

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
        private String _extensions;
        private ISourceConrolTools _tools;

        public ISourceConrolTools Tools
        {
            set
            {
                _tools = value;
                SetKeys();
                LoadOptions();
            }
        }

        private void OnGUI()
        {
            if (_tools != null)
            {
                EditorGUILayout.LabelField("Path to " + _executable);
                GUILayout.BeginHorizontal();
                _path = EditorGUILayout.TextField(_path);
                if (GUILayout.Button("Choose", GUILayout.Width(BUTTONWIDTH)))
                {
                    var startPath = String.IsNullOrEmpty(_path) ? String.Empty : _path;
                    var path = EditorUtility.OpenFilePanel("Set path to " + _executable, startPath, _extensions);
                    if (!String.IsNullOrEmpty(path))
                        _path = path;
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
        }

        public void SetKeys()
        {
            _pathKey = _tools.PathKey;
            _loginKey = _tools.LoginKey;
            _passwordKey = _tools.PasswordKey;
            _executable = _tools.Executable;
            _extensions = RuntimePlatformHelper.GetCurrentPlatform() == RuntimePlatform.Windows ? "exe" : String.Empty;
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