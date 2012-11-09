using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Globalization;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that attempts to dummy in publish receipts for items along a date range
    /// </summary>
    public class DefaultPublishReceipts : ModelTransformQueryTask
    {
        public string StartFormat { get; set; }
        public string Start { get; set; }
        public string Interval { get; set; }

        DateTime? _StartDateTime = null;
        public DateTime StartDateTime
        {
            get
            {
                if (_StartDateTime == null)
                {
                    DateTime dateTime;
                    if (string.IsNullOrWhiteSpace(StartFormat))
                    {
                        if (DateTime.TryParse(Start ?? DateTime.Now.ToString(), out dateTime))
                        {
                            _StartDateTime = dateTime;
                        }
                        else
                        {
                            _StartDateTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        if (DateTime.TryParseExact(Start ?? DateTime.Now.ToString(), StartFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTime))
                        {
                            _StartDateTime = dateTime;
                        }
                        else
                        {
                            _StartDateTime = DateTime.Now;
                        }
                    }
                }
                return _StartDateTime.Value;
            }
        }

        TimeSpan? _IntervalTimeSpan = null;
        public TimeSpan IntervalTimeSpan { get {
            if (_IntervalTimeSpan == null)
            {
                TimeSpan timeSpan;
                if (TimeSpan.TryParse(Interval ?? "-7.00:00:00", out timeSpan))
                {
                    _IntervalTimeSpan = timeSpan;
                }
                else
                {
                    _IntervalTimeSpan = TimeSpan.FromDays(-7);
                }
            }
            return _IntervalTimeSpan.Value;
        } }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<TaskItemTransform> items)
        {
            DateTimeOffset start = StartDateTime;

            foreach (TaskItemTransform item in items)
            {
                if (!File.Exists(item.Output.ItemSpec))
                {
                    item.Output.SetMetadata("Received", start.ToString());
                    item.Output.Save(Log, start.LocalDateTime);
                }
                start = start.Add(IntervalTimeSpan);
            }
        }
    }
}
