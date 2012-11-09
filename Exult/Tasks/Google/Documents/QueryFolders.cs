//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Build.Utilities;
//using Microsoft.Build.Framework;
//using Google.Documents;
//using Google.GData.Client;
//using Google.GData.Extensions;
//using Google.GData.Documents;
//using System.Text.RegularExpressions;
//using System.IO;
//using Microsoft.Win32;
//using System.Collections;
//using Exult.Model;

//namespace Exult.Tasks.Google.Documents
//{
//    /// <summary>
//    /// A task to list collections available to a user
//    /// </summary>
//    public class QueryFolders : DocumentsTask
//    {
//        public string TargetDirectory
//        {
//            get;
//            set;
//        }

//        public string Pattern
//        {
//            get;
//            set;
//        }

//        Regex _PatternExpression = null;
//        protected Regex PatternExpression
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(Pattern))
//                    return null;
//                if (_PatternExpression == null)
//                {
//                    _PatternExpression = new Regex(Pattern);
//                    Log.LogMessage(MessageImportance.Normal, "Listing Collections that match '{0}'", Pattern);
//                }
//                return _PatternExpression;
//            }
//        }

//        [Required]
//        public ITaskItem FolderQuery
//        {
//            get;
//            set;
//        }

//        [Output]
//        public ITaskItem[] Folders
//        {
//            get;
//            set;
//        }

//        private IEnumerable<DocumentEntry> GetEntries(FolderQuery query)
//        {
//            DocumentsService service = GetDocumentsService();
//            DocumentsFeed feed = null;
//            while(true)
//            {
//                feed = service.Query(query);
//                Log.LogMessage(MessageImportance.Normal, "Retrieved {0} Entries", feed.Entries.Count);
//                foreach (DocumentEntry entry in feed.Entries)
//                {
//                    yield return entry;
//                }
//                if (feed.NextChunk == null)
//                {
//                    yield break;
//                }
//                query = new FolderQuery() { Uri = new Uri(feed.NextChunk) };
//            }   
//        }
        
//        public override bool Execute()
//        {
//            FolderQuery query = new FolderQuery();
//            if (this.FolderQuery.Exists())
//            {
//                //Only get folders that have changed since last time
//                query.EditedMin = this.FolderQuery.GetTimestamp();
//                Log.LogMessage(MessageImportance.Normal, "Searching for directories updated after {0}", query.EditedMin);
//            }

//            List<DocumentEntry> documents = GetEntries(query).ToList();
//            IDictionary<string, DocumentEntry> documentDictionary = documents.ToDictionary(item => item.ResourceId);
//            List<ITaskItem> folders = new List<ITaskItem>();
//            DateTime updated = DateTime.MinValue;
//            foreach (DocumentEntry entry in documents)
//            {
//                if (entry.Updated > updated)
//                {
//                    updated = entry.Updated;
//                }
//                List<MappedPath> paths = GetPaths(entry, documentDictionary).ToList();

//                //handle each path, as we may allow multiple locations for a collection
//                foreach (MappedPath path in paths)
//                {
//                    if (Pattern == null || PatternExpression.IsMatch(path.TitlePath))
//                    {
//                        Log.LogMessage(MessageImportance.High, "Matched \"{0}\"", path.TitlePath);
//                        folders.Add(BuildFolder(entry, path));
//                    }
//                    else
//                    {
//                        Log.LogMessage(MessageImportance.Low, "Skipped \"{0}\"", path.TitlePath);
//                    }
//                }
//            }
//            if (updated > DateTime.MinValue)
//            {
//                FolderQuery.Save(Log, updated);
//            }
//            Folders = folders.ToArray();
//            return true;
//        }

//        private IEnumerable<MappedPath> GetPaths(DocumentEntry entry, IDictionary<string, DocumentEntry> documentDictionary)
//        {
//            if (entry == null)
//            {
//                yield break;
//            }
//            if (entry.ParentFolders.Any())
//            {
//                foreach (AtomLink parentFolder in entry.ParentFolders)
//                {
//                    DocumentEntry parent = null;
//                    if (!documentDictionary.TryGetValue(parentFolder.AbsoluteUri, out parent))
//                    {
//                        continue;
//                    }
//                    foreach (MappedPath path in GetPaths(parent, documentDictionary))
//                    {
//                        yield return new MappedPath(path, entry);
//                    }
//                }
//            }
//            else
//            {
//                yield return new MappedPath(entry);
//            }
//        }


//        private TaskItem BuildFolder(DocumentEntry document, MappedPath mappedPath)
//        {
//            string targetFile = Path.Combine(TargetDirectory, mappedPath.MappedPath);

//            TaskItem folder = new TaskItem(targetFile);

//            if (File.Exists(targetFile))
//            {
//                DateTime updated = File.GetLastWriteTime(targetFile);
//                Log.LogMessage(MessageImportance.Normal, "Exists at \"{0}\"", targetFile);
//                if (updated != document.Updated)
//                {
//                    Log.LogMessage(MessageImportance.Low, "Updated - Local: {0} Remote: {1}", updated, document.Updated);
//                }
//                else
//                {
//                    Log.LogMessage(MessageImportance.Low, "Updated - {0}", document.Updated);
//                }
//            }
//            else
//            {
//                Log.LogMessage("Found new folder");
//            }

//            folder.FillMetadata(document);
//            folder.FillMetadata(mappedPath);
//            folder.Save(Log, document.Updated);

//            return folder;
//        }
//    }
//}
