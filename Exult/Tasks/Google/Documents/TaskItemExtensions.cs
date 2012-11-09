using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using System.Xml.Linq;
using Google.Documents;
using Google.GData.Documents;
using System.Collections;
using Microsoft.Build.Utilities;

namespace Exult.Tasks.Google.Documents
{
    public static class TaskItemExtensions
    {
        public static Document.DocumentType RequireDocumentType(this ITaskItem item)
        {
            string documentTypeName = item.GetMetadata("DocumentType");
            if (string.IsNullOrWhiteSpace(documentTypeName))
            {
                throw new ArgumentException("metadata does not define DocumentType");
            }

            Document.DocumentType documentType;
            if (!Enum.TryParse<Document.DocumentType>(documentTypeName, out documentType))
            {
                throw new ArgumentException("metadata has invalid DocumentType");
            }
            return documentType;
        }

        public static ResourceType RequireResourceType(this ITaskItem item)
        {
            string resourceTypeName = item.GetMetadata("ResourceType");
            if (string.IsNullOrWhiteSpace(resourceTypeName))
            {
                throw new ArgumentException("metadata does not define ResourceType");
            }

            ResourceType resourceType;
            if (!Enum.TryParse<ResourceType>(resourceTypeName, out resourceType))
            {
                throw new ArgumentException("metadata has invalid ResourceType");
            }
            return resourceType;
        }

        public static void RequireDocumentType(this ITaskItem item, Document.DocumentType requiredDocumentType)
        {
            Document.DocumentType documentType = item.RequireDocumentType();

            if (documentType != requiredDocumentType)
            {
                throw new ArgumentException(string.Format("Document was of type {0} but must be of type {1}", documentType, requiredDocumentType));
            }
        }

        public static string RequireExportUri(this ITaskItem item)
        {
            string exportUri = item.GetMetadata("ExportUri");
            if (string.IsNullOrWhiteSpace(exportUri))
            {
                throw new ArgumentException("metadata does not define ExportUri");
            }
            return exportUri;
        }

        public static string RequireResourceId(this ITaskItem item)
        {
            string resourceId = item.GetMetadata("ResourceId");
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentException("metadata does not define ResourceId");
            }
            return resourceId;
        }

        public static IEnumerable<string> RequireParentFolders(this ITaskItem item)
        {
            string parentFolders = item.GetMetadata("ParentFolders") ?? "";
            return parentFolders.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string RequireTitle(this ITaskItem item)
        {
            string title = item.GetMetadata("Title");
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("metadata does not define Title");
            }
            return title;
        }

        public static DateTime RequireUpdated(this ITaskItem item)
        {
            string updated = item.GetMetadata("Updated");
            DateTime result;
            if (DateTime.TryParse(updated, out result))
            {
                return result;
            }
            throw new ArgumentException("metadata does not define valid 'Updated' DateTime");
        }

        public static string RequireSelf(this ITaskItem item)
        {
            string self = item.GetMetadata("Self");
            if (string.IsNullOrWhiteSpace(self))
            {
                throw new ArgumentException("metadata does not define Self");
            }
            return self;
        }

        public static string RequireTitlePath(this ITaskItem item)
        {
            string titlePath = item.GetMetadata("TitlePath");
            if (string.IsNullOrWhiteSpace(titlePath))
            {
                throw new ArgumentException("metadata does not define TitlePath");
            }
            return titlePath;
        }

        public static void CopyMetadataTo(this ITaskItem source, ITaskItem destination, string prefix)
        {
            foreach (string key in source.CloneCustomMetadata().Keys.OfType<string>())
            {
                destination.SetMetadata(String.Concat(prefix, key), source.GetMetadata(key));
            }
        }

        public static ITaskItem FillMetadata(this ITaskItem item, PathMapping mapping)
        {
            item.SetMetadata("MappedExtension", mapping.MappedExtension ?? "");
            item.SetMetadata("MappedFilename", mapping.MappedFilename ?? "");
            item.SetMetadata("MappedPath", mapping.MappedPath);
            item.SetMetadata("ResourceType", mapping.ResourceType.ToString());
            item.SetMetadata("Title", mapping.Title ?? "");
            item.SetMetadata("TitlePath", mapping.TitlePath ?? "");
            return item;
        }

        public static ITaskItem FillMetadata(this ITaskItem item, Document document)
        {
            //Find and use first number as an index
            string filenameIndex = new string(
                (document.Title ?? "")
                .SkipWhile(c => !char.IsDigit(c))
                .TakeWhile(c => char.IsDigit(c))
                .ToArray());
            int index = 0;
            int.TryParse(filenameIndex, out index);

            item.SetMetadata("TitleIndex", index.ToString());

            item.SetMetadata("Author", document.Author ?? "");
            if (document.AccessControlList != null)
            {
                item.SetMetadata("AccessControlList", document.AccessControlList.ToString() ?? "");
            }

            if (document.RevisionDocument != null)
            {
                item.SetMetadata("RevisionDocument", document.RevisionDocument.ToString() ?? "");
            }

            if (document.LastModified != null)
            {
                item.SetMetadata("LastModifiedName", document.LastModified.Name ?? "");
                item.SetMetadata("LastModifiedEmail", document.LastModified.EMail ?? "");
            }

            if (document.ParentFolders != null)
            {
                item.SetMetadata("ParentFolders", string.Join(";",document.ParentFolders.ToArray()));
            }
            item.SetMetadata("QuotaBytesUsed", document.QuotaBytesUsed.ToString());

            item.SetMetadata("LastViewed", document.LastViewed.ToString());

            item.SetMetadata("Title", document.Title ?? "");
            item.SetMetadata("ETag", document.ETag ?? "");
            item.SetMetadata("Id", document.Id ?? "");
            item.SetMetadata("DocumentType", document.Type.ToString());
            item.SetMetadata("ResourceId", document.ResourceId ?? "");
            item.SetMetadata("Summary", document.Summary ?? "");
            item.SetMetadata("Self", document.Self ?? "");
            if (document.DocumentEntry != null)
            {
                item.SetMetadata("Description", document.DocumentEntry.Description ?? "");
            }
            item.SetMetadata("Updated", document.Updated.ToString());
            return item;
        }

    }
}
