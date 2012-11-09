using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that gives the name of the recursive dir (trims trailing separator)
    /// </summary>
    public class RecursiveDirName : BaseTask
    {
        [Required]
        public ITaskItem[] Inputs
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] Outputs
        {
            get;
            set;
        }

        public override bool Execute()
        {
            List<ITaskItem> outputs = new List<ITaskItem>(Inputs.Length);
            foreach (ITaskItem item in Inputs)
            {
                TaskItem output = new TaskItem(item);
                output.SetMetadata("RecursiveDirName", (item.GetMetadata("RecursiveDir") ?? "").TrimEnd('/', '\\'));
                outputs.Add(output);
            }
            Outputs = outputs.ToArray();
            return true;
        }
       
    }
}
