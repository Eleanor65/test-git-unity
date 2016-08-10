﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl.Svn
{
    public class SvnOptionsWindow : EditorWindow
    {
        private const int BUTTONWIDTH = 70;

        private string _svnPath;
        private string _login;
        private string _password;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Path to svn.exe");
            GUILayout.BeginHorizontal();
            _svnPath = EditorGUILayout.TextField(_svnPath);
            if (GUILayout.Button("Choose", GUILayout.Width(BUTTONWIDTH)))
            {
                _svnPath = EditorUtility.OpenFolderPanel("Choose folder with svn.exe", @"C:\Program Files\TortoiseSVN\bin", "");
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
                SaveOptions();
                this.Close();
            }
        }

        public void LoadOptions()
        {
            _svnPath = EditorPrefs.GetString(SvnTools.PATHKEY);
            _login = EditorPrefs.GetString(SvnTools.LOGINKEY);
            _password = EditorPrefs.GetString(SvnTools.PASSWORDKEY);
        }

        private void SaveOptions()
        {
            if (!String.IsNullOrEmpty(_svnPath) && Directory.Exists(_svnPath))
            {
                EditorPrefs.SetString(SvnTools.PATHKEY, _svnPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid path!", "This path is invalid! Please, choose correct path", "OK");
                return;
            }

            if (!String.IsNullOrEmpty(_login))
                EditorPrefs.SetString(SvnTools.LOGINKEY, _login);
            else
                EditorPrefs.DeleteKey(SvnTools.LOGINKEY);

            if (!String.IsNullOrEmpty(_password))
                EditorPrefs.SetString(SvnTools.PASSWORDKEY, _password);
            else
                EditorPrefs.DeleteKey(SvnTools.PASSWORDKEY);

            EditorUtility.DisplayDialog("Success!", "Options were successfully saved.", "OK");
        }
    }
}