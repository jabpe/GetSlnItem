using System;
using System.Collections.Generic;
using System.IO;

namespace GetSlnItem
{
    public class VirtualPath
    {
        public List<string> PathParts { get; private set; }

        public VirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                PathParts = new List<string>(0);
            }
            else
            {
                var split = path.Split(Path.DirectorySeparatorChar);
                var altSplit = path.Split(Path.AltDirectorySeparatorChar);

                PathParts = altSplit.Length > split.Length ? new List<string>(altSplit) : new List<string>(split);

                // Optional DirSeparatorChar at the beginning of file path
                if (string.IsNullOrEmpty(PathParts[0]))
                {
                    PathParts.RemoveAt(0);
                }

                if (PathParts.Count - 1 > 0 && string.IsNullOrEmpty(PathParts[PathParts.Count - 1]))
                {
                    PathParts.RemoveAt(PathParts.Count - 1);
                }

                if (PathParts.Contains(null) || PathParts.Contains(""))
                    throw new ArgumentException("All directory and file parts of the path must be non-empty.");
            }
        }

        public override string ToString()
        {
            return (PathParts.Count==0) ? Path.DirectorySeparatorChar.ToString()
                :string.Concat(Path.DirectorySeparatorChar.ToString(),
                    string.Join(Path.DirectorySeparatorChar.ToString(), PathParts),
                    Path.DirectorySeparatorChar.ToString());
        }
    }
}
