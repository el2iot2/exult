using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using HtmlAgilityPack;
using Exult.Tasks.Html;


namespace Exult.Test.Html
{
    [TestClass]
    public class InternalStyleSheetRemoveTests
    {
        TestTaskLoggingHelper Log = new TestTaskLoggingHelper();

        [TestMethod]
        public void RemoveAll_SingleStyle()
        {
            string document = 
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>body{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";
               
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveAll(doc);

            StringWriter writer = new StringWriter();
            doc.Save(writer);

            Assert.IsFalse(writer.ToString().Contains("style"));
        }


        [TestMethod]
        public void RemoveAll_MultipleStyles()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>body{color:#000000;font-size:11pt;}</style><style type='text/css'>body2{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveAll(doc);

            StringWriter writer = new StringWriter();
            doc.Save(writer);

            Assert.IsFalse(writer.ToString().Contains("style"), "should not contain 'style'");
        }


        [TestMethod]
        public void RemoveSelectors_SingleStyle_RemoveSoleSelector()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>body{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveSelectors(Log, doc, new[] { new RemoveStyles.SelectorInfo(Log, "body") });

            StringWriter writer = new StringWriter();
            doc.Save(writer);

            Assert.IsFalse(writer.ToString().Contains("style"), "should not contain 'style'");
        }

        [TestMethod]
        public void RemoveSelectors_SingleStyle_RemoveOneOfTwoSelectors()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>hat{color:#000000;font-size:11pt;}coat{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveSelectors(Log, doc, new[] { new RemoveStyles.SelectorInfo(Log, "hat") });

            StringWriter writer = new StringWriter();
            doc.Save(writer);
            Console.WriteLine(writer.ToString());
            Assert.IsTrue(writer.ToString().Contains("style"), "should contain 'style'");
            Assert.IsTrue(writer.ToString().Contains("coat"), "should contain 'coat'");
            Assert.IsFalse(writer.ToString().Contains("hat"), "should not contain 'hat'");
        }

        [TestMethod]
        public void RemoveSelectors_SingleStyle_RemoveBothSelectors()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>hat{color:#000000;font-size:11pt;}coat{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveSelectors(Log, doc, new[] { new RemoveStyles.SelectorInfo(Log, "hat"), new RemoveStyles.SelectorInfo(Log, "coat") });

            StringWriter writer = new StringWriter();
            doc.Save(writer);
            Console.WriteLine(writer.ToString());
            Assert.IsFalse(writer.ToString().Contains("style"), "should not contain 'style'");
            Assert.IsFalse(writer.ToString().Contains("coat"), "should not contain 'coat'");
            Assert.IsFalse(writer.ToString().Contains("hat"), "should not contain 'hat'");
        }

        [TestMethod]
        public void RemoveSelectors_TwoStyles_RemoveBothSelectors()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>hat{color:#000000;font-size:11pt;}</style><style type='text/css'>coat{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveSelectors(Log, doc, new[] { new RemoveStyles.SelectorInfo(Log, "hat"), new RemoveStyles.SelectorInfo(Log, "coat") });

            StringWriter writer = new StringWriter();
            doc.Save(writer);
            Console.WriteLine(writer.ToString());
            Assert.IsFalse(writer.ToString().Contains("style"), "should not contain 'style'");
            Assert.IsFalse(writer.ToString().Contains("coat"), "should not contain 'coat'");
            Assert.IsFalse(writer.ToString().Contains("hat"), "should not contain 'hat'");
        }

        [TestMethod]
        public void RemoveSelectors_TwoStyles_RemoveHatSelector()
        {
            string document =
"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" + @"
<html><head><title>title</title><style type='text/css'>hat{color:#000000;font-size:11pt;}</style><style type='text/css'>coat{color:#000000;font-size:11pt;}</style></head><body><p>paragraph</p></body></html>";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(document);

            RemoveStyles.RemoveSelectors(Log, doc, new[] { new RemoveStyles.SelectorInfo(Log, "hat") });

            StringWriter writer = new StringWriter();
            doc.Save(writer);
            Console.WriteLine(writer.ToString());
            Assert.IsTrue(writer.ToString().Contains("style"), "should contain 'style'");
            Assert.IsTrue(writer.ToString().Contains("coat"), "should contain 'coat'");
            Assert.IsFalse(writer.ToString().Contains("hat"), "should not contain 'hat'");
        }
        
    }
}
