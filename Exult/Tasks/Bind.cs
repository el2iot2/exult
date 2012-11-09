using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public class Bind : BaseTask
    {
        [Required]
        public ITaskItem[] Models
        {
            get;
            set;
        }

        [Required]
        public ITaskItem ViewModel
        {
            get;
            set;
        }

        [Required,Output]
        public ITaskItem[] Views
        {
            get;
            set;
        }

        public override bool Execute()
        {
            DateTime viewModelTimestamp = ViewModel.GetTimestamp();
            Log.LogMessage(MessageImportance.High, "Bind:");
            Log.LogMessage(MessageImportance.Normal, "ViewModel: {0}", ViewModel.ItemSpec);

            WarnIfUneven(Tuple.Create("Models", Models), Tuple.Create("Views", Views));
            foreach (var tuple in Zip(Models,Views))
            {
                ITaskItem model = tuple.Item1;
                ITaskItem view = tuple.Item2;

                Log.LogMessage(MessageImportance.Normal, "Model: {0}", model.ItemSpec);
                Log.LogMessage(MessageImportance.Normal, "View: {0}", view.ItemSpec);

                DateTime modelTimestamp = model.GetTimestamp();

                view.RequireParentDirectory(Log);

                if (!XslTransforms.Bind(Log, modelPath: model.ItemSpec, viewModelPath: ViewModel.ItemSpec, viewPath: view.ItemSpec))
                    return false;

                HtmlDocument doc = new HtmlDocument();
                doc.Load(view.ItemSpec);
                HtmlNode html = doc.DocumentNode.SelectSingleNode("//html");
                if (html != null)
                {
                    HtmlAttribute xvm = html.Attributes.FirstOrDefault(item => item.Name == "xmlns:xvm");
                    if (xvm != null)
                    {
                        xvm.Remove();
                        doc.Save(view.ItemSpec);
                    }
                }

                if (viewModelTimestamp > modelTimestamp)
                {
                    view.Touch(viewModelTimestamp);
                }
                else
                {
                    view.Touch(modelTimestamp);
                }

            }
            return true;
        }
    }
}
