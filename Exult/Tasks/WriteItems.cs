using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Exult.Schema;

namespace Exult.Tasks
{
    /// <summary>
    /// A task to index task items into a single 
    /// </summary>
    public class WriteItems : Task
    {
        [Required]
        public ITaskItem[] Inputs
        {
            get;
            set;
        }

        [Output]
        public ITaskItem Output
        {
            get;
            set;
        }

        public override bool Execute()
        {
            Items items = new Items();
            foreach (ITaskItem input in Inputs)
            {
                items.Item.AddItemRow(
                    input.GetMetadata("FullPath"),
                    input.GetMetadata("RootDir"),
                    input.GetMetadata("FileName"),
                    input.GetMetadata("Extension"),
                    input.GetMetadata("RelativeDir"),
                    input.GetMetadata("Directory"),
                    input.GetMetadata("RecursiveDir"),
                    input.GetMetadata("Identity"),
                    input.GetMetadata("ModifiedTime"),
                    input.GetMetadata("CreatedTime"),
                    input.GetMetadata("AccessedTime")
                );
            }
            items.WriteXml(Output.ItemSpec);
            return true;
        }
    }
}
