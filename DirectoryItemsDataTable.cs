using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Build.Construction;

namespace GetSlnItem
{

    class DirectoryItemsDataTable
    {
        private readonly bool directory, file;
        private readonly string virtualPath;
        public DataTable Table { get; private set; }

        public DirectoryItemsDataTable(string virtualPath, bool directory, bool file)
        {
            this.virtualPath = virtualPath;
            Table = new DataTable(virtualPath);
            this.directory = directory;
            this.file = file;
            Table.Columns.Add(new DataColumn("Name", typeof(string)));
            Table.Columns.Add(new DataColumn("VirtualDirectory", typeof(string)));
            Table.Columns.Add(new DataColumn("Path", typeof(string)));
            Table.Columns.Add(new DataColumn("ItemType", typeof(string)));
            Table.Columns.Add(new DataColumn("ProjectDependencies", typeof(List<string>)));
        }

        public void AddItems(IList<ProjectInSolution> items, IReadOnlyList<ProjectInSolution> projectsInSolution)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                var row = Table.NewRow();
                row["Name"] = item.ProjectName;
                row["Path"] = System.IO.File.Exists(item.AbsolutePath) ? item.RelativePath : null;
                row["ItemType"] = item.ProjectType == SolutionProjectType.SolutionFolder
                    ? "Directory"
                    : item.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat ? "Project" : "File";
                row["VirtualDirectory"] = virtualPath;

                // include project dependencies;
                // get names by guid
                row["ProjectDependencies"] = item.Dependencies
                    .Select(x => projectsInSolution.SingleOrDefault(p => p.ProjectGuid == x)?.ProjectName)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if ((!directory && !file)
                    || (directory && row["ItemType"].ToString() == "Directory")
                    || (file && (row["ItemType"].ToString() == "Project" || row["ItemType"].ToString() == "File")))
                    Table.Rows.Add(row);
            }
        }

    }
}
