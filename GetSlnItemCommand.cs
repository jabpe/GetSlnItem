using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.Build.Construction;

namespace GetSlnItem
{
    [Cmdlet(VerbsCommon.Get, "SlnItem")]
    public class GetSlnItemCommand : PSCmdlet
    {
        private string slnPath;
        private VirtualPath virtualPath;
        private bool directory, file, recurse;

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "SlnPath to a VS solution file."
            )]
        public string SlnPath
        {
            get { return slnPath; }

            set
            {
                var path = SessionState.Path.GetUnresolvedProviderPathFromPSPath(value);

                if (!System.IO.File.Exists(path))
                {
                    throw new FileNotFoundException();
                }

                if (!Path.HasExtension(path) || Path.GetExtension(path) != ".sln")
                {
                    throw new FileLoadException("Only .sln file extensions supported.");
                }

                slnPath = path;
            }
        }

        [Parameter(
             Position = 1,
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "Path within the solution file internal virtual file structure."
         )]
        public string VirtualPath { get; set; }

        [Parameter(
         )]
        public SwitchParameter Directory
        {
            get { return directory;  }
            set { directory = value; }
        }

        [Parameter(
         )]
        public SwitchParameter File
        {
            get { return file; }
            set { file = value; }
        }

        [Parameter(
         )]
        public SwitchParameter Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }

        protected override void ProcessRecord()
        {
            
            virtualPath = new VirtualPath(VirtualPath);

            var solutionFile = SolutionFile.Parse(SlnPath);
            string currDirGuid = null;

            // Set currDirGuid to last dir in VirtualPath, if found in solution
            if (!string.IsNullOrEmpty(virtualPath.ToString()))
            {
                foreach (var nextItem in virtualPath.PathParts)
                {
                    try
                    {
                        currDirGuid =
                            solutionFile.ProjectsInOrder
                                .Where(project => project.ParentProjectGuid == currDirGuid)
                                .Single(project => project.ProjectName.Trim().ToLower() == nextItem.Trim().ToLower())
                                .ProjectGuid;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new ArgumentException(string.Format(
                            "The {0} in virtual path {1} not found in SLN structure.", nextItem, virtualPath));
                    }
                }
            }

            var dirStack = new Stack<Tuple<string, string>>();
            dirStack.Push(new Tuple<string, string>(currDirGuid, virtualPath.ToString()));

            var tables = new DataSet("Directories");

            while (dirStack.Count > 0)
            {
                currDirGuid = dirStack.Peek().Item1;
                var currDirVirtPath = dirStack.Peek().Item2;
                dirStack.Pop();

                if (recurse)
                {
                    foreach (var nextDir in solutionFile.ProjectsInOrder
                        .Where(project => project.ProjectType == SolutionProjectType.SolutionFolder
                                          && project.ParentProjectGuid == currDirGuid).ToList())
                    {
                        // Push next folders to recurse to to stack
                        dirStack.Push(new Tuple<string, string>(nextDir.ProjectGuid,
                            string.Concat(currDirVirtPath,
                            nextDir.ProjectName,
                            Path.DirectorySeparatorChar.ToString())));
                    }
                }

                var dir = new DirectoryItemsDataTable(currDirVirtPath, directory, file);

                dir.AddItems(solutionFile.ProjectsInOrder
                    .Where(project => project.ParentProjectGuid == currDirGuid)
                    .ToList());

                tables.Tables.Add(dir.Table);
            }

            WriteObject(tables.Tables);
        }
    }
}
