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
    /// A task that appends together two Models
    /// </summary>
    public class Zip : BaseTask
    {
        [Required]
        public ITaskItem[] Prefixes
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Suffixes
        {
            get;
            set;
        }

        [Required, Output]
        public ITaskItem[] Outputs
        {
            get;
            set;
        }

        public override bool Execute()
        {
            WarnIfUneven(Tuple.Create("Prefixes", Prefixes), Tuple.Create("Suffixes", Suffixes), Tuple.Create("Outputs", Outputs));
            foreach (var tuple in Zip(Prefixes, Suffixes, Outputs))
            {
                ITaskItem prefix = tuple.Item1;
                ITaskItem suffix = tuple.Item2;
                ITaskItem output = tuple.Item3;

                DateTime prefixUpdated = prefix.GetTimestamp();
                DateTime suffixUpdated = suffix.GetTimestamp();

                Log.LogMessage(MessageImportance.High, "Appending");
                Log.LogMessage(MessageImportance.Normal, "Prefix: {0}", Path.GetFileName(prefix.ItemSpec));
                Log.LogMessage(MessageImportance.Normal, "Suffix: {0}", Path.GetFileName(suffix.ItemSpec));
                Log.LogMessage(MessageImportance.Normal, "Output: {0}", Path.GetFileName(output.ItemSpec));
                Log.LogMessage(MessageImportance.Normal, "Directory: {0}", Path.GetDirectoryName(prefix.ItemSpec));

                JoinModels.Join(
                    log:Log, 
                    models: new[] { tuple.Item1, tuple.Item2 }, 
                    output: tuple.Item3);

                if (prefixUpdated > suffixUpdated)
                {
                    File.SetLastWriteTime(output.ItemSpec, prefixUpdated);
                }
                else
                {
                    File.SetLastWriteTime(output.ItemSpec, suffixUpdated);
                }
            }
            
            return true;
        }
    }
}
