using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Google.Documents;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Documents;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Win32;
using System.Collections;
using Exult.Model;

namespace Exult.Tasks.Google.Documents
{
    /// <summary>
    /// A task to list collections available to a user
    /// </summary>
    public class GetFolders : DocumentsTask, ICancelableTask
    {
        bool _Cancelled = false;

        public string TargetDirectory
        {
            get;
            set;
        }

        public string Pattern
        {
            get;
            set;
        }

        Regex _PatternExpression = null;
        protected Regex PatternExpression
        {
            get
            {
                if (string.IsNullOrEmpty(Pattern))
                    return null;
                if (_PatternExpression == null)
                {
                    _PatternExpression = new Regex(Pattern);
                    Log.LogMessage(MessageImportance.Normal, "Listing Collections that match '{0}'", Pattern);
                }
                return _PatternExpression;
            }
        }

        [Output]
        public ITaskItem[] Folders
        {
            get;
            set;
        }

        public override bool Execute()
        {
            GDataCredentials credentials = GetDataCredentials();
            RequestSettings settings = new RequestSettings("code.google.com/p/exult/", credentials);
            settings.AutoPaging = true;
            settings.PageSize = 100;

            DocumentsRequest request = new DocumentsRequest(settings);
            Feed<Document> feed = request.GetFolders();

            List<ITaskItem> outputs = new List<ITaskItem>();

            // this takes care of paging the results in
            List<Document> entries = feed.Entries.ToList();
            IDictionary<string, Document> documentDictionary = entries.ToDictionary(item => item.Self);

            RequireDirectory(TargetDirectory);

            foreach (Document entry in entries)
            {
                if (_Cancelled)
                {
                    return false;
                }

                List<PathMapping> paths = GetPaths(entry, documentDictionary).ToList();

                //handle each path, as we may allow multiple locations for a collection
                foreach (PathMapping path in paths)
                {
                    if (Pattern == null || PatternExpression.IsMatch(path.TitlePath))
                    {
                        Log.LogMessage(MessageImportance.High, "Matched \"{0}\"", path.TitlePath);
                        outputs.Add(BuildFolder(entry, path));
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.Low, "Skipped \"{0}\"", path.TitlePath);
                    }
                }
                
            }
            Folders = outputs.ToArray();
            return true;
        }

        private IEnumerable<PathMapping> GetPaths(Document entry, IDictionary<string, Document> lookup)
        {
            if (entry == null)
            {
                yield break;
            }
            
            ITaskItem entryItem = new TaskItem().FillMetadata(entry);

            if (entry.ParentFolders.Any())
            {
                foreach (string parentFolder in entry.ParentFolders)
                {
                    Document parent = null;
                    if (!lookup.TryGetValue(parentFolder, out parent))
                    {
                        continue;
                    }
                    foreach (PathMapping parentMapping in GetPaths(parent, lookup))
                    {
                        yield return new PathMapping(parentMapping, entryItem);
                    }
                }
            }
            else
            {
                yield return new PathMapping(entryItem);
            }
        }

        private TaskItem BuildFolder(Document model, PathMapping mappedPath)
        {
            string targetFile = Path.ChangeExtension(Path.Combine(TargetDirectory, mappedPath.MappedPath), mappedPath.ResourceType.ToString());
            
            TaskItem folder = new TaskItem(targetFile);

            if (File.Exists(targetFile))
            {
                DateTime updated = File.GetLastWriteTime(targetFile);
                Log.LogMessage(MessageImportance.Normal, "Exists at \"{0}\"", targetFile);
                
                if (updated != model.Updated)
                {
                    Log.LogMessage(MessageImportance.Low, "Updated - Local: {0} Remote: {1}", updated, model.Updated);
                }
                else
                {
                    Log.LogMessage(MessageImportance.Low, "Updated - {0}", model.Updated);
                }
            }
            else
            {
                Log.LogMessage("Found new folder");
            }

            folder.FillMetadata(model);
            folder.FillMetadata(mappedPath);
            folder.Save(Log, model.Updated);

            return folder;
        }

        public void Cancel()
        {
            _Cancelled = true;
        }
    }
}
