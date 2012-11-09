using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Google.GData.Client;
using Microsoft.Build.Framework;
using Google.Documents;
using System.Text.RegularExpressions;
using Google.GData.Documents;

namespace Exult.Tasks.Google.Documents
{
    public class PathMapping
    {
        static Regex MatchIllegalChars;
        static Regex MatchMultipleUnderscores;
        static PathMapping()
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + " " + Path.PathSeparator + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar + ".'\"";
            MatchIllegalChars = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            MatchMultipleUnderscores = new Regex("__+");
        }

        public virtual string SanitizeTitle(string title)
        {
            title = title.ToLower();
            title = title.Replace("'", "");
            return MatchMultipleUnderscores.Replace(MatchIllegalChars.Replace(title, "_"), "_");
        }

        public PathMapping(PathMapping parentMapping, ITaskItem model)
        {
            if (parentMapping != null)
            {
                if (parentMapping.DocumentType != Document.DocumentType.Folder)
                {
                    throw new ArgumentException("Path should have 'Folder' Document Type", "folderPath");
                }
                ParentMapping = parentMapping;
            }

            ResourceType = GetResourceType(model.RequireResourceId());

            Title = model.RequireTitle();

            switch (ResourceType)
            {
                case ResourceType.file:
                    MappedFilename = SanitizeTitle(Path.GetFileNameWithoutExtension(Title));
                    MappedExtension = String.Concat(".", ResourceType.ToString(), "_", SanitizeTitle(Path.GetExtension(Title).TrimStart('.')));
                    break;
                case ResourceType.folder:
                    MappedFilename = SanitizeTitle(Title);
                    MappedExtension = null;
                    break;
                default:
                    MappedFilename = SanitizeTitle(Title);
                    MappedExtension = string.Concat(".",ResourceType.ToString());
                    break;
            }
        }

        public PathMapping(ITaskItem entry) : this(null, entry)
        {
        }

        public PathMapping(string parentFolder)
        {
            ParentMapping = null;
            ResourceType = ResourceType.folder;

            if (!Directory.Exists(parentFolder))
            {
                throw new DirectoryNotFoundException(parentFolder);
            }

            MappedFilename = parentFolder;
            MappedExtension = null;
        }

        public ResourceType GetResourceType(string resourceId)
        {
            var parts = resourceId.Split(':');
            if (parts.Any())
            {
                string prefix = parts[0];
                return (ResourceType)Enum.Parse(typeof(ResourceType), prefix);
            }
            else
            {
                return ResourceType.unknown;
            }
        }

        public string ResourceIdToFolderName(string resourceId)
        {
            string[] parts = resourceId.Split(':');
            return HttpUtility.UrlEncode(parts[1]);
        }

        public Document.DocumentType DocumentType
        {
            get { return (Document.DocumentType)(int)ResourceType; }
        }

        public string MappedPath
        {
            get
            {
                string directoryName = "";
                if (ParentMapping != null)
                {
                    directoryName = ParentMapping.MappedPath ?? "";
                }
                string path = Path.Combine(directoryName, MappedFilename);
                string extension = (MappedExtension ?? "").Trim();
                if (string.IsNullOrEmpty(extension))
                {
                    return path;
                }
                return Path.ChangeExtension(path, extension);
            }
        }

        public string TitlePath
        {
            get
            {
                if (ParentMapping != null && !string.IsNullOrWhiteSpace(ParentMapping.TitlePath))
                {
                    return string.Concat(ParentMapping.TitlePath, "/", Title);
                }
                return Title;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - \"{1}\"", MappedPath, TitlePath);
        }

        public string MappedDirectoryName { get; private set; }
        public string MappedExtension { get; private set; }
        public PathMapping ParentMapping { get; private set; }
        public ResourceType ResourceType { get; private set; }
        public string Title { get; private set; }
        public string MappedFilename { get; private set; }
    }      
}

