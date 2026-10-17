// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using System;
using System.IO;

namespace BrawlLib.Internal.IO
{
    /// <summary>Independent persistent data for the portable product; no inherited BrawlCrate settings.</summary>
    public static class FighterPortPaths
    {
        public static string DataRoot
        {
            get
            {
                string configured = Environment.GetEnvironmentVariable("FIGHTERPORT_DATA_ROOT");
                if (string.IsNullOrWhiteSpace(configured))
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
                if (configured.Length < 3 || !char.IsLetter(configured[0]) || configured[1] != ':' ||
                    (configured[2] != '\\' && configured[2] != '/'))
                    throw new IOException("FIGHTERPORT_DATA_ROOT must be an absolute local directory.");
                return SourceWriteGuard.NormalizePath(configured);
            }
        }
    }
}
