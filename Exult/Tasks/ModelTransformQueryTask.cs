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
    public abstract class ModelTransformQueryTask : ModelQueryTask<TaskItemTransform>
    {
        [Required, Output]
        public ITaskItem[] Outputs
        {
            get;
            set;
        }

        public override IEnumerable<TaskItemTransform> Items
        {
            get {
                WarnIfUneven(Tuple.Create("Models", Models), Tuple.Create("Outputs", Outputs));
                return Zip(Models, Outputs).Select(item => new TaskItemTransform(item.Item1, item.Item2)); }
        }
        public override ITaskItem ToInputModel(TaskItemTransform item)
        {
            return item.Input;
        }
    }
}
