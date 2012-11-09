using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace Exult.Tasks.Aws
{
    /// <summary>
    /// A task that determines what should be uploaded to S3
    /// </summary>
    public class CalculateS3Puts : BaseTask
    {
        [Required]
        public ITaskItem[] LocalItems
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] S3ObjectTimestamps
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Receipts
        {
            get;
            set;
        }

        [Required, Output]
        public ITaskItem[] S3Puts
        {
            get;
            set;
        }

        public override bool Execute()
        {
            WarnIfUneven(Tuple.Create("LocalItems", LocalItems), Tuple.Create("S3ObjectTimestamps", S3ObjectTimestamps), Tuple.Create("Receipts", Receipts), Tuple.Create("S3Puts", S3Puts));
            foreach (var tuple in Zip(LocalItems, S3ObjectTimestamps, Receipts, S3Puts))
            {
                ITaskItem publish = tuple.Item1;
                ITaskItem published = tuple.Item2;
                ITaskItem receipt = tuple.Item3; 
                ITaskItem put = tuple.Item4;
                DateTime publishUpdated = publish.GetTimestamp();

                bool newPublish = !File.Exists(published.ItemSpec);

                if (newPublish || //If this is a new publish
                    publishUpdated > published.GetTimestamp()) //Or there is an update
                {
                    put.RequireParentDirectory(Log);

                    File.Copy(publish.ItemSpec, put.ItemSpec, true);
                    put.Touch(publishUpdated);
                }

                //Add a publish receipt for building the RSS feed
                if (newPublish)
                {
                    receipt.Save(Log, publishUpdated);
                }
            }
            return true;
        }
       
    }
}
