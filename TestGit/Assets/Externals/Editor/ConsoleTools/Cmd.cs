﻿using System;
using System.Collections.Generic;
using DTI.SourceControl.Svn;
using UnityEditor;

namespace DTI.SourceControl.ConsoleTools
{
	internal class Cmd
	{
		private readonly ConsoleTool _tool;

		public Cmd()
		{
		    _tool = new ConsoleTool(new ProcessLauncher(), new RegexParser());
		}

		public int Timeout
		{
			get { return _tool.Launcher.Timeout; }
			set { _tool.Launcher.Timeout = value; }
		}

		public string Command
		{
			set { _tool.Launcher.CommandName = value; }
		}

		public string Args
		{
			set { _tool.Launcher.Arguments = value; }
		}

		public string[] Patterns
		{
			set { ((RegexParser) _tool.Parser).Patterns = value; }
		}

	    public string BaseDirectory
	    {
            set { _tool.Launcher.BaseDirectory = value; }
	    }

		public List<String> Run()
		{
			var result = _tool.Run();
		    if (!result.Succeeded)
		    {
		        EditorUtility.DisplayDialog("Commit Failed!", result.ErrorMessage, "OK");
		        throw new Exception(result.ErrorMessage);
		    }
		    if (result.OutResult != null)
		        return result.OutResult;

            return new List<String>();
		}
	}
}
