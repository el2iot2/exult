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
    /// A task that links groupings of models with "prev" and "next" data
    /// </summary>
    public class RelateModels : ModelTransformQueryTask
    {
        bool? _Wrap = null;
        /// <summary>
        /// Gets or sets a value indicating whether to "wrap" the relations of the first and last model .
        /// </summary>
        /// <value>
        ///   <c>true</c> if the "prev" of the first model should be the last model and the "next" of the last model should be the first; otherwise, <c>false</c>.
        /// </value>
        public bool Wrap { get { return _Wrap != null && _Wrap.Value; } set { _Wrap = value; } }

        string _TargetNextAttributeName = "NextItemSpec";
        public string TargetNextAttributeName { get { return _TargetNextAttributeName; } set { _TargetNextAttributeName = value; } }

        string _TargetPrevAttributeName = "PrevItemSpec";
        public string TargetPrevAttributeName { get { return _TargetPrevAttributeName; } set { _TargetPrevAttributeName = value; } }

        string _SourceNextAttributeXPath = null;
        public string SourceNextAttributeXPath { get { return _SourceNextAttributeXPath; } set { _SourceNextAttributeXPath = value; } }

        string _SourcePrevAttributeXPath = null;
        public string SourcePrevAttributeXPath { get { return _SourcePrevAttributeXPath; } set { _SourcePrevAttributeXPath = value; } }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<TaskItemTransform> transformSequence)
        {
            List<TaskItemTransform> transforms = transformSequence.ToList();

            TaskItemTransform thisTransform = null;
            TaskItemTransform nextTransform = null;
            TaskItemTransform prevTransform = null;

            int next, prev;

            for (int i = 0; i < transforms.Count; i++)
            {
                thisTransform = transforms[i];

                next = i + 1;
                if (next < transforms.Count)
                {
                    nextTransform = transforms[next];
                }
                else if (Wrap)
                {
                    next = next % transforms.Count;
                    nextTransform = transforms[next];
                }
                else
                {
                    nextTransform = null;
                }

                prev = i - 1;
                if (prev >= 0)
                {
                    prevTransform = transforms[prev];
                }
                else if (Wrap)
                {
                    prev = (prev + transforms.Count) % transforms.Count;
                    prevTransform = transforms[prev];
                }
                else
                {
                    prevTransform = null;
                }
                
                XmlDocument thisDocument = new XmlDocument();
                thisDocument.Load(thisTransform.Input.ItemSpec);

                if (prevTransform != null)
                {
                    XmlAttribute attr = thisDocument.CreateAttribute(TargetPrevAttributeName);
                    if (SourcePrevAttributeXPath != null)
                    {
                        attr.Value = GetProperty("Getting \"Prev\" Attribute", prevTransform.Input, SourcePrevAttributeXPath);
                    }
                    else
                    {
                        attr.Value = prevTransform.Input.ItemSpec;
                    }
                    thisDocument.DocumentElement.Attributes.SetNamedItem(attr);
                }

                if (nextTransform != null)
                {
                    XmlAttribute attr = thisDocument.CreateAttribute(TargetNextAttributeName);
                    
                    if (SourceNextAttributeXPath != null)
                    {
                        attr.Value = GetProperty("Getting \"Next\" Attribute", prevTransform.Input, SourceNextAttributeXPath);
                    }
                    else
                    {
                        attr.Value = nextTransform.Input.ItemSpec;
                    }

                    thisDocument.DocumentElement.Attributes.SetNamedItem(attr);
                }

                thisDocument.Save(thisTransform.Output.ItemSpec);

            }
        }
    }
}
