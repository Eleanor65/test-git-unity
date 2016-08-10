﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DTI.SourceControl
{
    public class FileStatus
    {
        private const String PATHGROUP = "path";

        public Status Status;
        public String FullPath;
        public String Name;
        public String Extension;
        public String RelativePath;
        public bool Commit;

        public FileStatus(string line)
        {
            var regex = new Regex("^([?!DAMC][ ]+)(?<path>[^ ].*)$");

            var match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                SetStatus(line);
                SetPath(path);
                return;
            }

            regex = new Regex("^svn: warning: W155010: The node '(?<path>.+)' was not found[.]$");
            match = regex.Match(line);
            if (match.Success)
            {
                var path = match.Groups[PATHGROUP].Value;
                Status = Status.NotFound;
                SetPath(path);
                return;
            }

            SetPath(line);
        }

        private void SetStatus(String line)
        {
            switch (line[0])
            {
                case '?':
                    Status = Status.NotUnderVC;
                    break;
                case '!':
                    Status = Status.Missing;
                    break;
                case 'D':
                    Status = Status.Deleted;
                    break;
                case 'A':
                    Status = Status.Added;
                    break;
                case 'M':
                    Status = Status.Modified;
                    break;
                case 'C':
                    Status = Status.Conflicted;
                    break;
            }
        }

        private void SetPath(String line)
        {
            FullPath = line;
            RelativePath = line.Substring(Path.GetDirectoryName(Application.dataPath).Length);
            Name = Path.GetFileName(line);
            Extension = Path.GetExtension(Name);
        }

        public static void UpdateList(List<FileStatus> dst, List<FileStatus> src)
        {
            dst = dst.Select(x =>
            {
                if (src.Any(y => y.FullPath.Equals(x.FullPath)))
                {
                    x.Status = src.First(y => y.FullPath.Equals(x.FullPath)).Status;
                }
                return x;
            }).ToList();
            dst.AddRange(src.Where(y => dst.All(x => !y.FullPath.Equals(x.FullPath))));

            //return dst;
        }
    }
}