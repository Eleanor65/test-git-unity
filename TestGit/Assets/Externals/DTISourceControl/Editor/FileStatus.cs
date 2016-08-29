using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DTI.SourceControl
{
    public class FileStatus
    {
        protected const string PATHGROUP = "path";

        public Status Status;
        public string FullPath;
        public string Name;
        public string Extension;
        public string RelativePath;
        public bool Commit;

        protected void SetPath(string line)
        {
            FullPath = line;
            RelativePath = line.Substring(Path.GetDirectoryName(Application.dataPath).Length);
            Name = Path.GetFileName(line);
            Extension = Path.GetExtension(Name);
        }

        public static List<FileStatus> UpdateList(List<FileStatus> dst, List<FileStatus> src)
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
            return dst;
        }
    }
}