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
    /// A task to list the documents available in a collection
    /// </summary>
    public class GetFolderContent : DocumentsTask, ICancelableTask
    {
        bool _Cancelled = false;
    
        public string Pattern
        {
            get;
            set;
        }

        public string FolderMetadataPrefix
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

        [Required]
        public ITaskItem[] Folders
        {
            get;
            set;
        }

        [Output, Required]
        public ITaskItem[] FolderListings
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] FolderContent
        {
            get;
            set;
        }

        public class Request : DocumentsRequest
        {
            public Request(RequestSettings settings) : base(settings)
            {

            }
            public Feed<Document> GetFolderContent(string resourceId)
            {
                string uri = String.Format(DocumentsListQuery.foldersUriTemplate, resourceId);
                DocumentsListQuery q = PrepareQuery<DocumentsListQuery>(uri);
                return PrepareFeed<Document>(q);
            }
        }

        public override bool Execute()
        {
            GDataCredentials credentials = GetDataCredentials();
            RequestSettings settings = new RequestSettings("code.google.com/p/exult/", credentials);
            settings.AutoPaging = true;
            settings.PageSize = 100;

            List<ITaskItem> folderContent = new List<ITaskItem>();
            
            WarnIfUneven(Tuple.Create("Folders", Folders), Tuple.Create("FolderListings", FolderListings));
            foreach (var tuple in Zip(Folders, FolderListings))
            {
                if (_Cancelled)
                {
                    return false;
                }

                ITaskItem folder = tuple.Item1;
                ITaskItem folderListing = tuple.Item2;

                folder.LoadCustomMetadata();
                folder.RequireDocumentType(Document.DocumentType.Folder);

                //yada/hrm.folder -> yada/hrm/
                string folderPath = Path.Combine(Path.GetDirectoryName(folder.ItemSpec), Path.GetFileNameWithoutExtension(folder.ItemSpec));
                RequireDirectory(folderPath);
                PathMapping folderMapping = new PathMapping(folderPath);

                Request request = new Request(settings);

                string resourceId = folder.RequireResourceId();

                Log.LogMessage(MessageImportance.High, "Getting Folder Content \"{0}\"", folder.RequireTitlePath());
                Feed<Document> feed = request.GetFolderContent(resourceId);             
                
                // this takes care of paging the results in
                List<Document> documents = feed.Entries.Where(item => item.Type != Document.DocumentType.Folder).ToList();
                Log.LogMessage(MessageImportance.Normal, "Found {0} Item(s)", documents.Count);

                DateTime folderTimestamp = folder.GetTimestamp();
                DateTime latestTimestamp = folderTimestamp;
                
                foreach (Document document in documents)
                {
                    if (_Cancelled)
                    {
                        return false;
                    }

                    if (document.Updated > latestTimestamp)
                    {
                        latestTimestamp = document.Updated;
                    }
                    if (Pattern == null || PatternExpression.IsMatch(document.Title))
                    {
                        Log.LogMessage(MessageImportance.Normal, "Matched \"{0}\"", document.Title);
                        folderContent.Add(BuildContent(folder, document, folderMapping));
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.Low, "Skipped \"{0}\"", document.Title);
                    }   
                }
                folder.CopyMetadataTo(folderListing);
                folderListing.Save(Log, latestTimestamp);
            }
            FolderContent = folderContent.ToArray();
            return true;
        }

        private TaskItem BuildContent(ITaskItem folder, Document document, PathMapping folderMapping)
        {
            PathMapping documentPath = new PathMapping(folderMapping, new TaskItem().FillMetadata(document));

            string targetFile = documentPath.MappedPath;

            TaskItem content = new TaskItem(targetFile);
            folder.CopyMetadataTo(content, FolderMetadataPrefix ?? "Folder");
            content.FillMetadata(documentPath);
            content.FillMetadata(document);

            content.RequireParentDirectory(Log);

            if (content.Exists())
            {
                DateTime updated = File.GetLastWriteTime(targetFile);
                Log.LogMessage(MessageImportance.Normal, "Exists at \"{0}\"", targetFile);
                if (updated != document.Updated)
                {
                    Log.LogMessage(MessageImportance.Low, "Updated - Local: {0} Remote: {1}", updated, document.Updated);
                }
                else
                {
                    Log.LogMessage(MessageImportance.Low, "Updated - {0}", document.Updated);
                }
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal, "Detected new document");
            }

            if (document.DocumentEntry != null && 
                document.DocumentEntry.Content != null &&
                document.DocumentEntry.Content.Src != null)
            {
                content.SetMetadata("ExportUri", document.DocumentEntry.Content.Src.ToString());
            }

            content.Save(Log, document.Updated);
            
            return content;
        }

        public void Cancel()
        {
            _Cancelled = true;
        }
    }
}
