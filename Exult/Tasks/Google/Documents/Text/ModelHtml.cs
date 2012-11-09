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
using System.Collections;
using Exult.Model;

namespace Exult.Tasks.Google.Documents.Text
{
    /// <summary>
    /// A task to turn a GData Document into an appropriate model
    /// </summary>
    public class ModelHtml : BaseTask
    {
        public string FolderUriAlias
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Downloads
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Documents
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Uris
        {
            get;
            set;
        }

        [Required, Output]
        public ITaskItem[] Models
        {
            get;
            set;
        }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Modeling Text Document(s)");
            WarnIfUneven(Tuple.Create("Downloads", Downloads), Tuple.Create("Documents", Documents), Tuple.Create("Uris", Uris), Tuple.Create("Models", Models));
            foreach (var tuple in Zip(Downloads, Documents, Uris, Models))
            {
                ITaskItem downloadInput = tuple.Item1;
                ITaskItem documentInput = tuple.Item2;
                ITaskItem uri = tuple.Item3;
                ITaskItem modelOutput = tuple.Item4;
                
                Log.LogMessage(MessageImportance.Normal, modelOutput.ItemSpec);
                modelOutput.RequireParentDirectory(Log);

                modelOutput.LoadCustomMetadataFrom(documentInput);

                Uri u = new Uri(uri.ItemSpec.Replace('\\', '/'));
                string us = u.ToString();

                modelOutput.SetMetadata("Uri", us);
                modelOutput.SetMetadata(FolderUriAlias ?? "FolderUri", us.Substring(0, us.Length - u.Segments.Last().Length));

                XslTransforms.Google.Documents.Text.ToModel(htmlPath: downloadInput.ItemSpec, modelPath: modelOutput.ItemSpec, metadata: modelOutput.GetAllMetadata());

                DateTime modified = downloadInput.GetTimestamp();
                File.SetLastWriteTime(modelOutput.ItemSpec, modified);
            }
            return true;
        }
    }
}
