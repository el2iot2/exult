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
using System.Collections;
using HtmlAgilityPack;

namespace Exult.Tasks.Html
{
    /// <summary>
    /// Removes one ore more css style from an inline style sheet
    /// </summary>
    public class RemoveStyles : BaseTask
    {
        [Required]
        public ITaskItem[] Inputs
        {
            get;
            set;
        }

        public string Selectors
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] Outputs
        {
            get;
            set;
        }

        public override bool Execute()
        {
            List<string> selectorNames = 
                (Selectors ?? string.Empty)
                .Split(';')
                .Select(item => item.Trim())
                .DefaultIfEmpty("*")
                .ToList();
            bool removeAll = false;
            if (selectorNames.Any(item => item == "*"))
            {
                removeAll = true;
            }

            List<SelectorInfo> selectors = selectorNames.Select(item => new SelectorInfo(Log, item)).ToList();

            WarnIfUneven(Tuple.Create("Inputs", Inputs), Tuple.Create("Outputs", Outputs));
            foreach (var tuple in Zip(Inputs, Outputs))
            {
                ITaskItem input = tuple.Item1;
                ITaskItem output = tuple.Item2;
            
                Log.LogMessage(MessageImportance.High, input.ItemSpec);
                bool updatedItem = false;
                HtmlDocument document = new HtmlDocument();
                document.Load(input.ItemSpec);

                if (removeAll)
                {
                    Log.LogMessage(MessageImportance.Normal, "Removing all <style/> tags");
                    updatedItem = RemoveAll(document);
                }
                else
                {
                    Log.LogMessage(MessageImportance.Normal, "Removing {0}", Selectors);
                    updatedItem = RemoveSelectors(Log, document, selectors);
                }

                output.RequireParentDirectory(Log);

                if (updatedItem)
                {
                    document.Save(output.ItemSpec);
                }
                else
                {
                    File.Copy(input.ItemSpec, output.ItemSpec,true);
                }
                input.CopyMetadataTo(output);
            }
            return true;
        }

        public class SelectorInfo
        {
            public SelectorInfo(TaskLoggingHelper log, string selector)
            {
                Name = selector;
                string expression;
                if (selector.StartsWith("@"))
                {
                    log.LogMessage(MessageImportance.Normal, "Processing <style/> tag #{0}");
                    expression = string.Concat(selector, @"[^\;]*\;");
                }
                else
                {
                    expression = string.Concat(selector, @"\s*\{[^\}]*\}");
                }
                    
                Expression = new Regex(expression);
                log.LogMessage(MessageImportance.Normal, "Style '{0}' Expression: \n{1}", Name, expression);
            }
            public string Name { get; set; }
            public Regex Expression { get; set; }
        }

        public static bool RemoveSelectors(TaskLoggingHelper log, HtmlDocument document, IEnumerable<SelectorInfo> selectors)
        {
            bool updatedItem = false;
            HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//style");
            int nodeNumber = 0;
            foreach (HtmlNode node in collection)
            {
                log.LogMessage(MessageImportance.Normal, "Processing <style/> tag #{0}", nodeNumber++);
                List<SelectorInfo> foundSelectors = selectors.Where(item => item.Expression.IsMatch(node.InnerText)).ToList();
                foreach (SelectorInfo foundSelector in foundSelectors)
                {
                    log.LogMessage(MessageImportance.Normal, "Found '{0}' selector. Removing all occurrences.", foundSelector.Name);
                    node.InnerHtml = foundSelector.Expression.Replace(node.InnerHtml, string.Empty);
                    updatedItem = true;
                }

                if (string.IsNullOrWhiteSpace(node.InnerHtml))
                {
                    log.LogMessage(MessageImportance.Normal, "No CSS Styles remain. Removing <style/>");
                    node.Remove();
                    updatedItem = true;
                }
            }
            return updatedItem;
        }

        public static bool RemoveAll(HtmlDocument document)
        {
            
            HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//style");
            foreach (HtmlNode node in collection)
            {
                node.Remove();
            }
            return collection.Any();
        }
    }
}
