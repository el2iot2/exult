using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.IO;
using Microsoft.Build.Framework;

namespace Exult.Tasks
{
    public abstract class BaseTask : Task
    {
        protected IEnumerable<Tuple<ITaskItem, ITaskItem, ITaskItem, ITaskItem>> Zip(IEnumerable<ITaskItem> column1, IEnumerable<ITaskItem> column2, IEnumerable<ITaskItem> column3, IEnumerable<ITaskItem> column4)
        {
            return Zip(column1, column2, column3).Zip(column4, (tuple, suffix) => Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, suffix));
        }

        protected IEnumerable<Tuple<ITaskItem, ITaskItem, ITaskItem>> Zip(IEnumerable<ITaskItem> column1, IEnumerable<ITaskItem> column2, IEnumerable<ITaskItem> column3)
        {
            return Zip(column1, column2).Zip(column3, (tuple, suffix) => Tuple.Create(tuple.Item1, tuple.Item2, suffix));
        }

        protected void WarnIfUneven(params Tuple<string, ITaskItem[]>[] columns)
        {
            if (columns.Any())
            {
                var counts = columns.Select(item => Tuple.Create(item.Item2.Count(), item.Item1));
                ILookup<int, string> partitions = counts.ToLookup(item => item.Item1, item => item.Item2);

                if (partitions.Count > 1)
                {
                    Log.LogWarning("Supplied Task Columns have differing numbers of rows");
                    foreach (IGrouping<int, string> partition in partitions)
                    {
                        Log.LogMessage(MessageImportance.Normal, "Column(s) {0} have {1} rows", string.Join(", ", partition.ToArray()), partition.Key);
                    }
                }
            }
        }

        protected IEnumerable<Tuple<ITaskItem, ITaskItem>> Zip(IEnumerable<ITaskItem> column1, IEnumerable<ITaskItem> column2)
        {
            return column1.Zip(column2, (prefix, suffix) => Tuple.Create(prefix, suffix));            
        }

        protected void RequireDirectory(string directory)
        {
            RequireDirectory(Log, directory);
        }

        public static void RequireDirectory(TaskLoggingHelper log, string directory)
        {
            string fullDirectory = Path.GetFullPath(directory);
            if (File.Exists(fullDirectory))
                throw new ArgumentException("Not a directory", "directory");
            if (!Directory.Exists(fullDirectory))
            {
                Directory.CreateDirectory(fullDirectory);
                log.LogMessage(MessageImportance.Low, "Created directory '{0}'", fullDirectory);
            }
        }
    }
}
