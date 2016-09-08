using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DTI.SourceControl
{
    public class FileStatus
    {
        protected const string PATHGROUP = "path";

        public Status Status { get; protected set; }
        public string FullPath { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string RelativePath { get; private set; }
        public bool Commit;

        protected void SetPath(string line)
        {
            FullPath = line;
            RelativePath = line.Substring(Path.GetDirectoryName(Application.dataPath).Length + 1);
            Name = Path.GetFileName(line);
            Extension = Path.GetExtension(Name);
        }

        public static List<FileStatus> UpdateList(List<FileStatus> dst, List<FileStatus> src)
        {
            dst = dst.Select(x =>
            {
                var sameFileStatus = src.FirstOrDefault(y => y.FullPath.Equals(x.FullPath));
                if (sameFileStatus != null)
                    x.Status = sameFileStatus.Status;
                return x;
            }).ToList();
            dst.AddRange(src.Where(y => dst.All(x => !y.FullPath.Equals(x.FullPath))));
            return dst;
        }

        public override string ToString()
        {
            return FullPath;
        }
    }
}