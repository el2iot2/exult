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
    /// A task to download google documents
    /// </summary>
    public class ListS3ObjectTimestamps : AwsCredentialsTask
    {
        [Required]
        public ITaskItem[] Buckets
        {
            get;
            set;
        }

        [Output, Required]
        public ITaskItem[] Folders
        {
            get;
            set;
        }

        public override bool Execute()
        {
            RequireSecretKey();
            AmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client(AccessKey, SecretKey);
            WarnIfUneven(Tuple.Create("Buckets", Buckets), Tuple.Create("Folders", Folders));
            foreach (var itemPair in Zip(Buckets, Folders))
            {
                string bucketName = itemPair.Item1.ItemSpec;
                string outputFolder = itemPair.Item2.ItemSpec;
                string filePath;
                string directory;

                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = bucketName;
                RequireDirectory(outputFolder);

                Log.LogMessage(MessageImportance.High, "Listing Bucket \"{0}\"", bucketName);
                
                using (ListObjectsResponse response = s3Client.ListObjects(request))
                {
                    foreach (S3Object entry in response.S3Objects)
                    {
                        filePath = Path.Combine(outputFolder, entry.Key.Replace('/','\\'));
                        directory = Path.GetDirectoryName(filePath);
                        Log.LogMessage(MessageImportance.Normal, "Listed \"{0}\"", filePath);
                        if (!filePath.EndsWith("\\"))
                        {
                            RequireDirectory(directory);
                            DateTimeOffset lastModified = DateTimeOffset.ParseExact(entry.LastModified, CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);                            
                            File.WriteAllText(filePath, entry.LastModified + " -> " + lastModified.LocalDateTime);
                            File.SetLastWriteTime(filePath, lastModified.LocalDateTime);
                        }
                    }
                }
            }
            return true;
        }
    }
}
