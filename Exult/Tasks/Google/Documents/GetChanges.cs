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
    public class GetChanges : DocumentsTask
    {
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
        public ITaskItem Output
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

            DocumentsService service = new DocumentsService("Exult");
            service.Credentials = credentials;
            
            // Instantiate a ChangesQuery object to retrieve changes.
            ChangesQuery query = new ChangesQuery();
            

            // Make a request to the API and get all changes.
            ChangesFeed feed = service.Query(query);

            // Iterate through all of the changes returned
            foreach (ChangeEntry entry in feed.Entries)
            {
                //if (Pattern == null || PatternExpression.IsMatch(entry.TitlePath))
                //{
                //    Log.LogMessage(MessageImportance.High, "Matched \"{0}\"", path.TitlePath);
                //    outputs.Add(BuildFolder(entry, path));
                //}
                //else
                //{
                //    Log.LogMessage(MessageImportance.Low, "Skipped \"{0}\"", path.TitlePath);
                //}

                
                // Print the title and changestamp of this document to the screen
                Log.LogMessage(MessageImportance.Normal, entry.Title.Text);
                Log.LogMessage(MessageImportance.Normal, entry.Changestamp);
            }

            return true;
        }

        

        //private TaskItem BuildFolder(Document document, MappedPath mappedPath)
        //{
        //    string targetFile = Path.Combine(TargetDirectory, mappedPath.FullSanitizedTitlePath);
        //    //Uri relativeUri = TargetDirectoryUri.MakeRelativeUri(new Uri(Path.GetFullPath(targetFile), UriKind.Absolute));

        //    TaskItem folder = new TaskItem(targetFile);
        //    //TaskItem folder = new TaskItem(Uri.UnescapeDataString(relativeUri.ToString()));

        //    if (File.Exists(targetFile))
        //    {
        //        DateTime updated = File.GetLastWriteTime(targetFile);
        //        Log.LogMessage(MessageImportance.Normal, "Exists at \"{0}\"", targetFile);
        //        if (updated != document.Updated)
        //        {
        //            Log.LogMessage(MessageImportance.Low, "Updated - Local: {0} Remote: {1}", updated, document.Updated);
        //        }
        //        else
        //        {
        //            Log.LogMessage(MessageImportance.Low, "Updated - {0}", document.Updated);
        //        }
        //    }
        //    else
        //    {
        //        Log.LogMessage("Found new folder");
        //    }
            
        //    folder.FillMetadata(document);
        //    folder.FillMetadata(mappedPath);
        //    folder.Save(document.Updated);

        //    return folder;
        //}
    }
}
