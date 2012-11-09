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
using Argotic.Syndication;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public abstract class SyndicateFeedTask : ModelQueryTask<Tuple<ITaskItem, ITaskItem, ITaskItem>>
    {

        [Required]
        public ITaskItem[] Receipts
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Contents
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

        public override IEnumerable<Tuple<ITaskItem, ITaskItem, ITaskItem>> Items
        {
            get 
            {
                WarnIfUneven(Tuple.Create("Models", Models), Tuple.Create("Receipts", Receipts), Tuple.Create("Contents", Contents));
                return Zip(Models, Receipts, Contents);
            }
        }

        public override ITaskItem ToInputModel(Tuple<ITaskItem, ITaskItem, ITaskItem> item)
        {
            return item.Item1;
        }
    }
}
