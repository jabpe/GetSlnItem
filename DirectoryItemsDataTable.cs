using System.Data;
using Microsoft.Build.Construction;

namespace GetSlnItem
{

    class DirectoryItemsDataTable
    {
        private readonly bool directory, file;
        public DataTable Table { get; private set; }

        public DirectoryItemsDataTable(bool directory, bool file)
        {
            Table = new DataTable("Directory");
            this.directory = directory;
            this.file = file;
            Table.Columns.Add(new DataColumn("Name"));
            Table.Columns.Add(new DataColumn("FullName"));
            Table.Columns.Add(new DataColumn("ItemType"));
        }

        public void addItem(ProjectInSolution item)
        {
            var row = Table.NewRow();
            row["Name"] = item.ProjectName;
            row["FullName"] = item.AbsolutePath;
            row["ItemType"] = item.ProjectType == SolutionProjectType.SolutionFolder ? "Directory"
                : item.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat ? "Project" : "File";

            if ((!directory && !file)
                        || (directory && row["ItemType"].ToString() == "Directory")
                        || (file && (row["ItemType"].ToString() == "Project" || row["ItemType"].ToString() == "File")))
                Table.Rows.Add(row);
        }

    }
}
