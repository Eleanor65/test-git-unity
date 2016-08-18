using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System;

namespace DTI.SourceControl.Git
{
    public class Branch
    {
        public String Name;
        public String FullName;
        public bool Local;
        public bool Current;

        public Branch(String line)
        {
            if (line.StartsWith("*"))
                Current = true;
            var regex = new Regex(@"^[ ]+remotes/origin/(?<name>\S+)$");
            var match = regex.Match(line);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                FullName = "origin/" + Name;
                Local = false;
                return;
            }

            regex = new Regex(@"^[*]?[ ]+(?<name>\S+)$");
            match = regex.Match(line);
            if (match.Success)
            {
                Name = match.Groups["name"].Value;
                FullName = Name;
                Local = true;
            }
        }
    }
}