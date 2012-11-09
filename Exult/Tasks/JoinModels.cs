using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public class JoinModels : ModelQueryTask<ITaskItem>
    {
        [Required]
        public ITaskItem Output
        {
            get;
            set;
        }

        public override IEnumerable<ITaskItem> Items
        {
            get { return Models; }
        }

        public override ITaskItem ToInputModel(ITaskItem item)
        {
            return item;
        }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<ITaskItem> items)
        {
            Join(GroupBy, groupByValue, Log, items, Output);
        }

        public static void Join(TaskLoggingHelper log, IEnumerable<ITaskItem> models, ITaskItem output)
        {
            Join(null, null, log, models, output);
        }

        public static void Join(string groupBy, string groupByValue, TaskLoggingHelper log, IEnumerable<ITaskItem> models, ITaskItem output)
        {
            XmlDocument modelDocument = new XmlDocument();
            modelDocument.LoadXml("<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'/>");
            log.LogMessage(MessageImportance.Normal, "Joining {0} Models:", models.Count());

            if (groupBy != null)
            {
                XmlAttribute groupByAttribute = modelDocument.CreateAttribute(groupBy);
                groupByAttribute.Value = groupByValue;
                modelDocument.DocumentElement.SetAttributeNode(groupByAttribute);
            }

            DateTime mostRecentlyUpdated = DateTime.MinValue;

            int joinOrdinal = 0;
            foreach(ITaskItem model in models)
            {
                DateTime updated = model.GetTimestamp();
                if (updated > mostRecentlyUpdated)
                {
                    mostRecentlyUpdated = updated;
                }

                model.SetMetadata("JoinOrdinal", joinOrdinal.ToString());
                log.LogMessage(MessageImportance.Normal, "[{0}] {1}", joinOrdinal, model.ItemSpec);

                XmlDocument subDocument = new XmlDocument();
                subDocument.Load(model.ItemSpec);
                foreach (XmlNode node in subDocument.DocumentElement.ChildNodes)
                {
                    XmlNode importedNode = modelDocument.ImportNode(node, true);
                    modelDocument.DocumentElement.AppendChild(importedNode);
                }

                foreach (XmlAttribute attribute in subDocument.DocumentElement.Attributes)
                {
                    XmlAttribute importedNode = modelDocument.ImportNode(attribute, true) as XmlAttribute;
                    modelDocument.DocumentElement.SetAttributeNode(importedNode);
                }

                XmlAttribute joinOrdinalAttribute = modelDocument.CreateAttribute("JoinOrdinal");
                joinOrdinalAttribute.Value = joinOrdinal.ToString();
                modelDocument.DocumentElement.SetAttributeNode(joinOrdinalAttribute);
                joinOrdinal++;
            }

            output.RequireParentDirectory(log);

            modelDocument.Save(output.ItemSpec);
            if (mostRecentlyUpdated > DateTime.MinValue)
            {
                File.SetLastWriteTime(output.ItemSpec, mostRecentlyUpdated);
            }
        }
    }
}
