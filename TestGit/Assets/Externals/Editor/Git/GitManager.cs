using System;
using System.Collections.Generic;
using System.Linq;
using DTI.SourceControl.ConsoleTools;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace DTI.SourceControl.Git
{
    public class GitManager
    {
        //TODO: Запихать кучу всего.
        public void GetStatus()
        {
            var cmd = new Cmd()
            {
                BaseDirectory = Application.dataPath,
                Command = EditorPrefs.GetString(GitTools.PATHKEY),
                Args = "status",
                Patterns = new []
                {
                    "^(?<skip>On branch .+)$",
                    "^(?<skip>Your branch is up-to-date with '.+'.)$",
                    "^(?<skip>nothing to commit, working tree clean)$",
                    //"^(?<skip>)$"
                }
            };

            var outResult = cmd.Run();
        }

        public void Fetch()
        {
            var cmd = new Cmd()
            {
                BaseDirectory = Application.dataPath,
                Command = EditorPrefs.GetString(GitTools.PATHKEY),
                Args = "fetch",
            };
            cmd.Run();
        }

        public IEnumerable<Branch> GetBranches()
        {
            var cmd = new Cmd()
            {
                BaseDirectory = Application.dataPath,
                Command = EditorPrefs.GetString(GitTools.PATHKEY),
                Args = "branch -a",
                Patterns = new []
                {
                    "^(?<skip>[ ]+remotes/origin/HEAD -> origin/.+)$",

                    @"^(?<out>[*]?[ ]+\S+)$",
                    //@"^(?<out>[*]?[ ]+remotes/origin/\w+)$",
                    //"^(?<out>)$",
                }
            };
            var outResult = cmd.Run();

            var branches = outResult.Select(x => new Branch(x)).ToList();
            return branches;
        }

    }
}