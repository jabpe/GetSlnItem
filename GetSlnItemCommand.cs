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
    public class GetSlnItemCommand : Cmdlet
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
                if (!System.IO.File.Exists(value))
                {
                    throw new FileNotFoundException();
                }

                if (!Path.HasExtension(value) || Path.GetExtension(value) != ".sln")
                {
                    throw new FileLoadException("Only .sln file extensions supported.");
                }
                
                slnPath = value;
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

            if (!string.IsNullOrEmpty(virtualPath.ToString()))
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

            // TODO: Recurse

            var items = solutionFile.ProjectsInOrder.Where(project => project.ParentProjectGuid == currDirGuid)
                                .ToList();

            var dir = new DirectoryItemsDataTable(directory, file);

            if (items.Count > 0)
                foreach (var item in items)
                {
                    dir.addItem(item);
                }

            WriteObject(dir.Table, true);
        }
    }
}
