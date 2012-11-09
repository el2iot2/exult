using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using Newtonsoft.Json;

namespace Exult.Tasks.Json
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public class SelectJson : ModelQueryTask<ITaskItem>
    {
        readonly List<Dictionary<string, object>> _Groupings = new List<Dictionary<string, object>>();
        readonly List<string> _PropertySelectors = new List<string>();
        readonly List<string> _GroupingPropertySelectors = new List<string>();

        [Required]
        public ITaskItem Output
        {
            get;
            set;
        }

        public string VariableName
        {
            get;
            set;
        }

        public string RowsAlias
        {
            get;
            set;
        }

        [Required]
        public string PropertySelectors
        {
            get;
            set;
        }

        public string GroupingPropertySelectors
        {
            get;
            set;
        }

        public override void OnBeforeExecute()
        {
            _PropertySelectors.AddRange(PropertySelectors.Split(';').Distinct().OrderBy(item => item));
            List<string> selectors = new List<string>();
            if (!string.IsNullOrWhiteSpace(GroupingPropertySelectors))
            {
               
                selectors.AddRange(GroupingPropertySelectors.Split(';'));
            }
            _GroupingPropertySelectors.AddRange(selectors.Distinct().OrderBy(item => item));
        }

        public override void OnAfterExecute()
        {
            if (_Groupings.Any())
            {
                JsonSerializer serializer = new JsonSerializer() { Formatting = Newtonsoft.Json.Formatting.Indented };
                using (StreamWriter textWriter = new StreamWriter(Output.ItemSpec))
                {
                    textWriter.WriteLine(string.Format("var {0} =", VariableName ?? "exultJsonData"));
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
                    try
                    {
                        if (_Groupings.Count() > 1)
                        {
                            serializer.Serialize(jsonWriter, _Groupings);
                        }
                        else
                        {
                            serializer.Serialize(jsonWriter, _Groupings.First());
                        }
                        textWriter.WriteLine(";");
                    }
                    finally
                    {
                        //Don't use "using" because we want to defer close to after semicolon
                        (jsonWriter as IDisposable).Dispose();
                    }
                }
            }
        }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<ITaskItem> items)
        {
            Dictionary<string, object> grouping = new Dictionary<string, object>();
            
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

            foreach (ITaskItem item in items)
            {
                Dictionary<string, string> row = new Dictionary<string, string>();

                foreach (string propertySelector in _PropertySelectors)
                {
                    row.Add(propertySelector, GetProperty(propertySelector, item, propertySelector));
                }
                rows.Add(row);
            }
            if (rows.Any())
            {
                if (groupByValue != null)
                {
                    foreach (string groupingPropertySelector in _GroupingPropertySelectors)
                    {
                        if (groupingPropertySelector == GroupBy)
                        {
                            grouping[GroupBy] = groupByValue;
                        }
                        else
                        {
                            grouping[groupingPropertySelector] = GetProperty(groupingPropertySelector, items.First(), groupingPropertySelector);
                        }
                    }
                }
                
                grouping[RowsAlias ?? "Rows"] = rows;
                _Groupings.Add(grouping);
            }
        }

        public override IEnumerable<ITaskItem> Items
        {
            get { return Models; }
        }

        public override ITaskItem ToInputModel(ITaskItem item)
        {
            return item;
        }
    }
}
