using System;
using System.Data;
using System.IO;
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
        public string VirtualPath
        {
            get { return virtualPath.ToString(); }
            set { virtualPath = new VirtualPath(value); }
        }

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
            var solutionFile = SolutionFile.Parse(SlnPath);
            var table = new DataTable("Sample Table");
            var column = new DataColumn("Name");
            table.Columns.Add(column);

            column = new DataColumn("Absolute Path");
            table.Columns.Add(column);

            foreach (var bla in solutionFile.ProjectsInOrder)
            {
                if (virtualPath == null)
                {
                    if (bla.ParentProjectGuid == null)
                    {
                        var row = table.NewRow();
                        row["Name"] = bla.ProjectName;
                        row["Absolute Path"] = bla.AbsolutePath;
                        table.Rows.Add(row);
                    }
                }
            }

            WriteObject(table, true);
        }
    }
}
