using UnityEditor;
using UnityEngine;
using System.Collections;

namespace DTI.SourceControl.Git
{
    public class BranchesWindow : EditorWindow
    {
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Branches!");
        }
    }
}