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
using Argotic.Common;

namespace Exult.Tasks
{
    /// <summary>
    /// A task that generates views by binding view model templates to model documents 
    /// </summary>
    public class SyndicateAtomFeed : SyndicateFeedTask
    {
        [Required]
        public string FeedId { get; set; }
        public string FeedTitle { get; set; }
        public string FeedAuthors { get; set; }
        public string FeedContributors { get; set; }
        public string FeedCategories { get; set; }
        public string FeedRights { get; set; }
        public string FeedIcon { get; set; }
        public string FeedLogo { get; set; }
        public string FeedSubtitle { get; set; }
        public string FeedLinkRelationSelf { get; set; }
        public string EntryTitleSelector { get; set; }
        public string EntryIdSelector { get; set; }
        public string EntrySummarySelector { get; set; }
        public string EntryContentEncoding { get; set; }
        public string EntryContentType { get; set; }
        public string EntryLinkAlternateSelector { get; set; }

        public override void ExecuteItemSequence(string groupByValue, IEnumerable<Tuple<ITaskItem, ITaskItem, ITaskItem>> items)
        {
            var feed = new AtomFeed
            {
                Id = new AtomId(new Uri(FeedId)),
                Title = new AtomTextConstruct(FeedTitle ?? ""),
                UpdatedOn = DateTime.Now,
            };

            if (!string.IsNullOrWhiteSpace(FeedRights))
            {
                feed.Rights = new AtomTextConstruct(FeedRights);
            }

            if (!string.IsNullOrWhiteSpace(FeedIcon))
            {
                feed.Icon = new AtomIcon(new Uri(FeedIcon));
            }

            if (!string.IsNullOrWhiteSpace(FeedLogo))
            {
                feed.Logo = new AtomLogo(new Uri(FeedLogo));
            }

            if (!string.IsNullOrWhiteSpace(FeedSubtitle))
            {
                feed.Subtitle = new AtomTextConstruct(FeedSubtitle);
            }
            

            if (!string.IsNullOrWhiteSpace(FeedAuthors))
            {
                foreach (string author in FeedAuthors.Split(';').Select(item => item.Trim()))
                {
                    feed.Authors.Add(new AtomPersonConstruct(author));
                }
            }

            if (!string.IsNullOrWhiteSpace(FeedContributors))
            {
                foreach (string contributor in FeedContributors.Split(';').Select(item => item.Trim()))
                {
                    feed.Contributors.Add(new AtomPersonConstruct(contributor));
                }
            }

            if (!string.IsNullOrWhiteSpace(FeedCategories))
            {
                foreach (string category in FeedCategories.Split(';').Select(item => item.Trim()))
                {
                    feed.Categories.Add(new AtomCategory(category));
                }
            }

            if (FeedLinkRelationSelf != null)
            {
                
                var selfLink = new AtomLink
                       {
                           Relation = "self",
                           Uri = new Uri(FeedLinkRelationSelf)
                       };
                feed.Links.Add(selfLink);
            }

            foreach (Tuple<ITaskItem, ITaskItem, ITaskItem> tuple in items.OrderByDescending(item => item.Item2.GetTimestamp()))
            {
                ITaskItem modelInput = tuple.Item1;
                ITaskItem receiptInput = tuple.Item2;
                ITaskItem contentInput = tuple.Item3;

                modelInput.LoadCustomMetadata();

                DateTime receiptModified = receiptInput.GetTimestamp();
                var entry = new AtomEntry
                {
                    Id = new AtomId(new Uri(modelInput.GetMetadata(EntryIdSelector ?? "Uri"))),
                    Title = new AtomTextConstruct(modelInput.GetMetadata(EntryTitleSelector ?? "Title")),
                    UpdatedOn = receiptModified,
                    Summary = new AtomTextConstruct(modelInput.GetMetadata(EntrySummarySelector ?? "Summary")),
                };

                if (string.IsNullOrWhiteSpace(entry.Title.Content))
                {
                    entry.Title.Content = tuple.Item1.ItemSpec;
                }
                if (string.IsNullOrWhiteSpace(entry.Summary.Content))
                {
                    entry.Summary.Content = entry.Title.Content;
                }

                if (contentInput.Exists())
                {
                    if (string.IsNullOrWhiteSpace(EntryContentEncoding))
                    {
                        entry.Content = new AtomContent(contentInput.ReadAllText());
                    }
                    else
                    {
                        entry.Content = new AtomContent(contentInput.ReadAllText(), EntryContentEncoding);
                    }

                    if (!string.IsNullOrWhiteSpace(EntryContentType))
                    {
                        entry.Content.ContentType = EntryContentType;
                    }
                }

                var alternateLink = new AtomLink
                {
                    Relation = "alternate",
                    Uri = new Uri(modelInput.GetMetadata(EntryLinkAlternateSelector ?? "Uri"))
                };
                entry.Links.Add(alternateLink);
                feed.AddEntry(entry);
            }
            using (FileStream stream = File.OpenWrite(Output.ItemSpec))
            {
                SyndicationResourceSaveSettings s = new SyndicationResourceSaveSettings() { CharacterEncoding = Encoding.UTF8 };
                feed.Save(stream, s);
            }
        }
    }
}
