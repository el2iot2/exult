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
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using System.Globalization;

namespace Exult.Tasks.Aws
{
    /// <summary>
    /// A task to put files to amazon s3
    /// </summary>
    public class S3Put : AwsCredentialsTask, ICancelableTask
    {
        [Required]
        public ITaskItem Bucket
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Puts
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Keys
        {
            get;
            set;
        }

        bool _Cancel = false;

        public override bool Execute()
        {
            RequireSecretKey();
            AmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client(AccessKey, SecretKey);
            string bucketName = Bucket.ItemSpec;
            Log.LogMessage(MessageImportance.High, "Connecting to \"{0}\"", bucketName);

            WarnIfUneven(Tuple.Create("Puts", Puts), Tuple.Create("Keys", Keys));
            foreach (var tuple in Zip(Puts, Keys))
            {
                ITaskItem put = tuple.Item1;
                ITaskItem keyItem = tuple.Item2;
                string key = keyItem.ItemSpec.Replace('\\', '/').TrimStart('/');
                if (!put.Exists())
                {
                    Log.LogMessage(MessageImportance.Normal, "Skipping {0} because it does not exist", key);
                    continue;
                }

                if (_Cancel)
                    return false;

                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = put.ItemSpec,
                    Key = key,
                    Timeout = -1,
                    ReadWriteTimeout = 300000     // 5 minutes in milliseconds
                };

                S3CannedACL cannedACL;
                if (Enum.TryParse<S3CannedACL>(put.GetMetadata("CannedACL") ?? "", out cannedACL))
                {
                    Log.LogMessage(MessageImportance.Normal, "Applying CannedACL: {0}", cannedACL);
                    putObjectRequest.CannedACL = cannedACL;
                }

                string contentType = put.GetMetadata("ContentType");
                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    Log.LogMessage(MessageImportance.Normal, "Applying ContentType: {0}", contentType);
                    putObjectRequest.ContentType = contentType;
                }

                Log.LogMessage(MessageImportance.High, "Putting \"{0}\"", key);

                using (var upload = s3Client.PutObject(putObjectRequest)) 
                {
                }
            }
            return true;
        }

        public void Cancel()
        {
            _Cancel = true;
        }
    }
}
