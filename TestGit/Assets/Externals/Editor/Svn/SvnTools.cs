﻿using System;
using UnityEditor;

namespace DTI.SourceControl.Svn
{
	internal static class SvnTools
	{
        public const string PATHKEY = "svnPath";
        public const string LOGINKEY = "svnLogin";
        public const string PASSWORDKEY = "svnPassword";

		public static string Svn
		{
			get
			{
				if (RuntimePlatformHelper.GetCurrentPlatform() == RuntimePlatform.Windows)
				{
					return "svn.exe";
				}
				else
				{
				    return "svn";
				}
			}
		}

        public static String SvnDirectory
        {
            get { return EditorPrefs.GetString(PATHKEY); }
        }
	}
}
