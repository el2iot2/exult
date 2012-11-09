using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using Exult.Model.Sitemaps;
using System.Globalization;
using System.Xml.Serialization;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public class Sitemap : BaseTask
    {
        [Required]
        public ITaskItem[] Inputs
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Locations
        {
            get;
            set;
        }

        [Required, Output]
        public ITaskItem Output
        {
            get;
            set;
        }

        public override bool Execute()
        {
            List<tUrl> urls = new List<tUrl>();
            DateTime mostRecentlyModified = DateTime.MinValue;

            WarnIfUneven(Tuple.Create("Inputs", Inputs), Tuple.Create("Locations", Locations));
            foreach (var tuple in Zip(Inputs, Locations))
            {
                tUrl url = new tUrl();
                url.loc = tuple.Item2.ItemSpec.Replace('\\', '/');
                DateTime lastmod = tuple.Item1.GetTimestamp();
                if (lastmod > mostRecentlyModified)
                {
                    mostRecentlyModified = lastmod;
                }
                //http://www.w3.org/TR/NOTE-datetime
                //http://stackoverflow.com/questions/7281995/how-can-i-get-this-datetime-format-in-net
                url.lastmod = lastmod.ToString( "yyyy-MM-ddTHH:mmK", CultureInfo.InvariantCulture );
                urls.Add(url);
            }

            urlset urlset = new urlset();
            urlset.url = urls.ToArray();
            Output.RequireParentDirectory(Log);
            XmlSerializer serializer = new XmlSerializer(typeof(urlset));
            using(Stream stream = File.OpenWrite(Output.ItemSpec))
            {
                serializer.Serialize(stream, urlset);
            }

            if (mostRecentlyModified > DateTime.MinValue)
            {
                File.SetLastWriteTime(Output.ItemSpec, mostRecentlyModified);
            }
            return true;
        }

        public static void Join(TaskLoggingHelper log, IEnumerable<ITaskItem> models, ITaskItem output, string orderBy = null, string orderByDescending = null, int? skip = null, int? take = null, TypeCode orderByTypeCodeEnum = TypeCode.Int32)
        {
            
        }
    }
}
