﻿using System;
using System.IO;

namespace DTI.SourceControl
{
    internal static class RuntimePlatformHelper
    {
        public static RuntimePlatform GetCurrentPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case System.PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications")
                        & Directory.Exists("/System")
                        & Directory.Exists("/Users")
                        & Directory.Exists("/Volumes"))
                        return RuntimePlatform.Mac;
                    else
                        return RuntimePlatform.Linux;

                case System.PlatformID.MacOSX:
                    return RuntimePlatform.Mac;

                default:
                    return RuntimePlatform.Windows;
            }
        }
    }
}