using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using HtmlAgilityPack;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public abstract class ModelQueryTask<TItem> : BaseTask
    {
        [Required]
        public ITaskItem[] Models
        {
            get;
            set;
        }

        bool _ExpectHtml = false;
        public bool ExpectHtml { get { return _ExpectHtml; } set { _ExpectHtml = value; } }

        public string OrderByTypeCode
        {
            get;
            set;
        }

        TypeCode? _OrderByTypeCode = null;
        public TypeCode? OrderByTypeCodeEnum
        {
            get {
                if (_OrderByTypeCode == null)
                {
                    _OrderByTypeCode = TypeCode.Int32;
                    if (!string.IsNullOrWhiteSpace(OrderByTypeCode))
                    {
                        TypeCode typeCode;
                        if (Enum.TryParse<TypeCode>(OrderByTypeCode, out typeCode))
                        {
                            _OrderByTypeCode = typeCode;
                        }
                    }
                }
                return _OrderByTypeCode; }
        }

        public ITaskItem ViewModel { get; set; }

        protected int? _Skip = null;
        public int Skip { get { return _Skip.HasValue ? _Skip.Value : default(int); } set { _Skip = value; }}

        protected int? _Take = null;
        public int Take { get { return _Take.HasValue ? _Take.Value : default(int); } set { _Take = value; } }

        public string Where { get; set; }
        public string In { get; set; }
        public string NotIn { get; set; }

        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public string OrderByDescending { get; set; }

        bool? _UseXPath = null;
        public bool UseXPath { get{
            if (_UseXPath == null)
            {
                var selectors = new[] { GroupBy, OrderBy, OrderByDescending };
                _UseXPath = selectors.Any(item => item != null && item.Contains('@'));
                if (_UseXPath.Value && !selectors.All(item => item == null || item.Contains('@')))
                {
                    throw new Exception("If one selector uses an XPath, then all must");
                }
            }
            return _UseXPath.Value;
        }}

        public abstract IEnumerable<TItem> Items { get; }

        public abstract ITaskItem ToInputModel(TItem item);

        public virtual void OnBeforeExecute()
        {

        }

        public virtual void OnAfterExecute()
        {

        }



        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            OnBeforeExecute();
            List<TItem> items = Items.ToList();
            if (!UseXPath)
            {
                var inputModels = items.Select(item => ToInputModel(item)).ToList();

                ITaskItem missing = inputModels.FirstOrDefault(item => !item.Exists());
                if (missing != null)
                {
                    Log.LogError("Input Model does not exist: '{0}'", missing.ItemSpec);
                    return false;
                }
                inputModels.ForEach(item => item.LoadCustomMetadata());
            }
            if (!String.IsNullOrWhiteSpace(GroupBy))
            {
                Log.LogMessage("Grouping By: {0}", GroupBy);
                foreach (IGrouping<string, TItem> grouping in items.GroupBy(item => GetProperty("Group By", ToInputModel(item), GroupBy)))
                {
                    ExecuteItemGrouping(grouping.Key, grouping);
                }
            }
            else
            {
                ExecuteItemGrouping(null, items);
            }
            OnAfterExecute();
            return true;
        }

        public virtual void ExecuteItemGrouping(string groupByValue, IEnumerable<TItem> items)
        {
            if (!String.IsNullOrWhiteSpace(OrderBy))
            {
                Log.LogMessage(MessageImportance.Low, "Ordering By: {0}", OrderBy);
                items = items.OrderBy(item => Convert.ChangeType(GetProperty("Order By", ToInputModel(item), OrderBy), OrderByTypeCodeEnum.Value));
            }
            else if (!String.IsNullOrWhiteSpace(OrderByDescending))
            {
                Log.LogMessage(MessageImportance.Low, "Ordering By (Descending): {0}", OrderByDescending);
                items = items.OrderByDescending(item => Convert.ChangeType(GetProperty("Order By Descending", ToInputModel(item), OrderByDescending), OrderByTypeCodeEnum.Value));
            }

            if (_Skip.HasValue)
            {
                Log.LogMessage(MessageImportance.Low, "Skipping first: {0}", Skip);
                items = items.Skip(Skip);
            }

            if (_Take.HasValue)
            {
                Log.LogMessage(MessageImportance.Low, "Taking first: {0}", Take);
                items = items.Take(Take);
            }
            ExecuteItemSequence(groupByValue, items);
        }

        public abstract void ExecuteItemSequence(string groupByValue, IEnumerable<TItem> items);


        protected string GetProperty(string purpose, ITaskItem model, string query)
        {
            if (UseXPath)
            {
                return GetPropertyViaXPath(purpose, model, query);
            }
            return model.GetMetadata(query);
        }

        protected string GetPropertyViaXPath(string purpose, ITaskItem model, string query)
        {
            Log.LogMessage(MessageImportance.Low, "Getting a value to {0} ({1})", purpose, query);
            XPathNavigator nav;
            if (ExpectHtml)
            {
                HtmlDocument document = new HtmlDocument();
                document.Load(model.ItemSpec);
                nav = document.CreateNavigator();
            }
            else
            {
                XPathDocument d = new XPathDocument(model.ItemSpec);
                nav = d.CreateNavigator();
            }
            XPathNodeIterator it = nav.Select(query);
            while (it.MoveNext())
            {
                return it.Current.Value;
            }
            return string.Empty;
        }
    }
}
