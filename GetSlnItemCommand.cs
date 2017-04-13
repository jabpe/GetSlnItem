using System.Data;
using System.IO;
using System.Management.Automation;
using Microsoft.Build.Construction;

namespace GetSlnItem
{
    [Cmdlet(VerbsCommon.Get, "SlnItem")]
    public class GetSlnItemCommand : Cmdlet
    {
        private string slnPath, virtualPath;

        [Parameter(
            ParameterSetName = "SlnPath",
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
                if (!File.Exists(value))
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
            ParameterSetName = "VirtualPath",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path within the solution file internal virtual file structure."
            )]
        public string VirtualPath { get; set; }

        protected override void ProcessRecord()
        {
            var solutionFile = SolutionFile.Parse(SlnPath);
            var items = new DataTable("Sample Table");
            var column = new DataColumn("Name");
            items.Columns.Add(column);

            column = new DataColumn("Absolute Path");
            items.Columns.Add(column);

            foreach (var bla in solutionFile.ProjectsInOrder)
            {
                if (virtualPath == null)
                {
                    if (bla.ParentProjectGuid == null)
                    {
                        var row = items.NewRow();
                        row["Name"] = bla.ProjectName;
                        row["Absolute Path"] = bla.AbsolutePath;
                        items.Rows.Add(row);
                    }
                }
            }

            WriteObject(items, true);

        }
    }
}
