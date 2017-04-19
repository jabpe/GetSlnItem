using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetSlnItem
{
    public class VirtualPath
    {
        private List<string> PathParts { get; set; }

        public IEnumerator GetEnumerator()
        {
            return PathParts.GetEnumerator();
        }

        public VirtualPath()
        {
            PathParts = new List<string>(0);
        }

        public VirtualPath(string path)
        {
            var split = path.Split(Path.DirectorySeparatorChar);
            var altSplit = path.Split(Path.AltDirectorySeparatorChar);

            PathParts = altSplit.Length > split.Length ? new List<string>(altSplit) : new List<string>(split);

            // Optional DirSeparatorChar at the beginning of file path
            if (PathParts[0] == "")
                PathParts.RemoveAt(0);

            if (PathParts.Contains(""))
                throw new ArgumentException("All directory and file parts of the path must be non-empty.");
        }

        public override string ToString()
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), PathParts);
        }
    }
}
