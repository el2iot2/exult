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
    /// A task that queries models and makes the results available to msbuidl via an output parameter
    /// </summary>
    public class CreateModelItem : ModelQueryTask<ITaskItem>
    {
        [Output]
        public ITaskItem[] Outputs
        {
            get;
            set;
        }

        List<ITaskItem> _Outputs;

        public override IEnumerable<ITaskItem> Items
        {
            get { return Models; }
        }

        public override ITaskItem ToInputModel(ITaskItem item)
        {
            return item;
        }

        public override bool Execute()
        {
            _Outputs = new List<ITaskItem>();
            bool result = base.Execute();
            Outputs = _Outputs.ToArray();
            return result;
        }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<ITaskItem> items)
        {
            _Outputs.AddRange(items);
        }

    }
}
