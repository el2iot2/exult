using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using System.Xml.Linq;
using System.Xml;
using Microsoft.Build.Utilities;

namespace Exult.Tasks
{
    public static class TaskItemExtensions
    {
        public static void LoadCustomMetadata(this ITaskItem taskItem)
        {
            taskItem.LoadCustomMetadataFrom(taskItem);
        }

        public static void LoadCustomMetadataFrom(this ITaskItem taskItem, ITaskItem sourceItem)
        {
            IDictionary<string, string> fileMetadata = Load(sourceItem.ItemSpec);
            foreach (string key in fileMetadata.Keys
                .Except(new[] //Exclude Well-Known Metadata
                    {
"FullPath",
"RootDir",
"Filename",
"Extension",
"RelativeDir",
"Directory",
"RecursiveDir",
"Identity",
"ModifiedTime",
"CreatedTime",
"AccessedTime"
                    }))
            {
                taskItem.SetMetadata(key, fileMetadata[key]);
            }
        }

        public static IDictionary<string, object> GetAllMetadata(this ITaskItem taskItem)
        {
            return taskItem
                .MetadataNames
                .OfType<string>()
                .Distinct()
                .OrderBy(item => item)
                .ToDictionary(
                    item => item, 
                    item => (object)taskItem.GetMetadata(item));
        }

        public static IDictionary<string, string> Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Cannot load model metadata", fileName);
            }
            XDocument document = XDocument.Load(fileName);
            IList<XAttribute> attrs = document.Root.Attributes().ToList();

            return document.Root
                .Attributes()
                .ToLookup(item => item.Name.LocalName, item => item.Value)
                .ToDictionary(item => item.Key, item => item.First());
        }
        public static void Touch(this ITaskItem taskItem, DateTime lastWriteTime)
        {
            File.SetLastWriteTime(taskItem.ItemSpec, lastWriteTime);
        }

        public static DateTime GetTimestamp(this ITaskItem taskItem)
        {
            if (!taskItem.Exists())
            {
                throw new FileNotFoundException("Cannot get last write time", taskItem.ItemSpec);
            }
            return File.GetLastWriteTime(taskItem.ItemSpec);
        }

        public static bool Exists(this ITaskItem taskItem)
        {
            return File.Exists(taskItem.ItemSpec);
        }

        public static bool DirectoryExists(this ITaskItem taskItem)
        {
            return Directory.Exists(taskItem.ItemSpec);
        }

        public static void RequireParentDirectory(this ITaskItem taskItem, TaskLoggingHelper log)
        {
            string fullPath = Path.GetFullPath(taskItem.ItemSpec);
            string dir = Path.GetDirectoryName(fullPath);
            BaseTask.RequireDirectory(log, dir);
        }

        public static string ReadAllText(this ITaskItem taskItem)
        {
            return File.ReadAllText(taskItem.ItemSpec);
        }

        public static void Save(this ITaskItem taskItem, TaskLoggingHelper log, DateTime? lastWriteTime = null)
        {   
            XNamespace xm = "http://code.google.com/p/exult/model";
            XElement document = new XElement(xm + "Document",
                new XAttribute(XNamespace.Xmlns + "xm", "http://code.google.com/p/exult/model"));

            document.Add(
                taskItem
                    .MetadataNames
                    .OfType<string>()
                    .Distinct()
                    .OrderBy(item => item)
                    .Select(item => new XAttribute(item, taskItem.GetMetadata(item)))
                    .ToArray());

            taskItem.RequireParentDirectory(log);

            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, NewLineOnAttributes = true };
            using (XmlWriter writer = XmlWriter.Create(taskItem.ItemSpec, settings))
            {
                document.Save(writer);
            }
            if (lastWriteTime != null)
            {
                taskItem.Touch(lastWriteTime.Value);
            }
        }
    }
}
