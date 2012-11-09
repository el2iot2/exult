using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Xml.Xsl;
using System.Xml.Schema;
using System.IO;
using HtmlAgilityPack;
using System.Xml.XPath;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Exult
{
    public static class XslTransforms
    {
        public class Parameter
        {
            public Parameter(string name, string namespaceUri, object value)
            {
                Name = name;
                NamespaceUri = namespaceUri;
                Value = value;
            }

            public string Name { get; private set; }
            public string NamespaceUri { get; private set; }
            public object Value { get; private set; }
        }

        static XslCompiledTransform _BindTransform = null;
        public static XslCompiledTransform BindTransform
        { 
            get 
            {
                return _BindTransform = _BindTransform ?? XslTransforms.CompileResourceTransform("Exult.Transform.Bind.xslt");
            }
        }

        public static class Google
        {
            public static class Documents
            {
                public static class Text
                {

                    static XslCompiledTransform _ToModelTransform = null;
                    public static XslCompiledTransform ToModelTransform
                    {
                        get
                        {
                            return _ToModelTransform = _ToModelTransform ?? XslTransforms.CompileResourceTransform("Exult.Transform.Google.Documents.Text.ToModel.xslt");
                        }
                    }

                    public static void ToModel(IXPathNavigable htmlNavigable, XmlWriter modelWriter, IDictionary<string, object> metadata, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
                    {
                        Parameter[] parameters = null;
                        if (metadata != null)
                        {
                            XmlDocument metadataDocument = new XmlDocument();
                            metadataDocument.LoadXml("<xm:document xmlns:xm='http://code.google.com/p/exult/model'/>");
                            foreach (KeyValuePair<string, object> pair in metadata)
                            {
                                metadataDocument.DocumentElement.SetAttribute(pair.Key, Convert.ToString(pair.Value));
                            }

                            parameters = new[] { new Parameter("metadata", "", metadataDocument) };
                        }

                        ToModelTransform.Apply(
                        inputNavigable:htmlNavigable,
                        outputWriter: modelWriter,
                            messageAction: messageAction,
                            parameters: parameters);

                    }

                    public static void ToModel(string htmlPath, string modelPath, IDictionary<string, object> metadata, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
                    {
                        using (FileStream htmlStream = new FileStream(htmlPath, FileMode.Open))
                        using (XmlWriter modelWriter = XmlWriter.Create(modelPath, ToModelTransform.OutputSettings))
                        {
                            HtmlDocument htmlDocument = new HtmlDocument();
                            htmlDocument.Load(htmlStream);
                            ToModel(htmlNavigable: htmlDocument, modelWriter: modelWriter, metadata: metadata, messageAction: messageAction);
                        }
                    }

                    public static string ToModel(string html, IDictionary<string, object> metadata, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
                    {
                        using (StringWriter modelStringWriter = new StringWriter())
                        using (XmlWriter modelWriter = XmlWriter.Create(modelStringWriter, ToModelTransform.OutputSettings))
                        {
                            HtmlDocument htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(html);
                            ToModel(htmlNavigable: htmlDocument, modelWriter: modelWriter, metadata: metadata, messageAction: messageAction);
                            return modelStringWriter.ToString();
                        }
                    }
                }
            }
        }

        public static void Apply(this XslCompiledTransform transform, IXPathNavigable inputNavigable, XmlWriter outputWriter, Action<object, XsltMessageEncounteredEventArgs> messageAction = null, IEnumerable<Parameter> parameters = null)
        {
            messageAction = messageAction ?? new Action<object, XsltMessageEncounteredEventArgs>((o, a) => { });
            XsltArgumentList al = new XsltArgumentList();
            if (parameters != null)
            {
                foreach (Parameter p in parameters)
                {
                    al.AddParam(p.Name, p.NamespaceUri, p.Value);
                }
            }

            al.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(delegate(object sender, XsltMessageEncounteredEventArgs e)
            {
                messageAction(sender, e);
            });

            transform.Transform(inputNavigable, al, outputWriter);
        }


        public static void Apply(this XslCompiledTransform transform, XmlReader inputReader, XmlWriter outputWriter, Action<object, XsltMessageEncounteredEventArgs> messageAction = null, IEnumerable<Parameter> parameters = null)
        {
            messageAction = messageAction ?? new Action<object, XsltMessageEncounteredEventArgs>((o, a) => { });
            XsltArgumentList al = new XsltArgumentList();
            if (parameters != null)
            {
                foreach (Parameter p in parameters)
                {
                    al.AddParam(p.Name, p.NamespaceUri, p.Value);
                }
            }

            al.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(delegate(object sender, XsltMessageEncounteredEventArgs e)
            {
                messageAction(sender, e);
            });

            transform.Transform(inputReader, al, outputWriter);
        }

        public static void Apply(this XslCompiledTransform transform, string inputPath, string outputPath, Action<object, XsltMessageEncounteredEventArgs> messageAction = null, IEnumerable<Parameter> parameters = null)
        {
            using (XmlReader inputReader = XmlReader.Create(inputPath))
            using (XmlWriter outputWriter = XmlWriter.Create(outputPath,transform.OutputSettings))
            {
                Apply(transform, inputReader, outputWriter, messageAction: messageAction, parameters: parameters);
            }
        }

        public static XslCompiledTransform CompileTransform(XmlReader stylesheetReader, XsltSettings settings = null, XmlUrlResolver resolver=null)
        {
            settings = settings ?? new XsltSettings(true, true);
            resolver = resolver ?? new XmlUrlResolver();
            XslCompiledTransform compiledTransform = new XslCompiledTransform();
            compiledTransform.Load(stylesheetReader, settings, resolver);
            return compiledTransform;
        }

        public static XslCompiledTransform CompileResourceTransform(string name, XsltSettings settings = null, XmlUrlResolver resolver = null)
        {
            using (XmlReader schemaReader = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
            {
                return CompileTransform(schemaReader, settings:settings, resolver:resolver);
            }
        }

        public static bool Bind(TaskLoggingHelper log, XmlReader modelReader, XmlReader viewModelReader, XmlWriter viewWriter, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
        {
            XmlDocument viewModelDocument = new XmlDocument();
            try
            {
                viewModelDocument.Load(viewModelReader);
            }
            catch (Exception x)
            {
                log.LogError("View Model: {0}", x.Message);
                return false;
            }

            XmlDocument modelDocument = new XmlDocument();

            try
            {
                modelDocument.Load(modelReader);
            }
            catch (Exception x)
            {
                log.LogError("Model: {0}", x.Message);
                return false;
            }

            using (StringReader dummyReader = new StringReader("<dummy/>"))
            {
                XslTransforms.BindTransform.Apply(
                    XmlReader.Create(dummyReader),
                    viewWriter,
                    messageAction: messageAction,
                    parameters: new[] { 
                    new XslTransforms.Parameter("ViewModel", "", viewModelDocument),
                    new XslTransforms.Parameter("Model", "", modelDocument)});
            }
            return true;
        }

        public static bool Bind(TaskLoggingHelper log, string modelPath, string viewModelPath, string viewPath, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
        {
            using (FileStream viewModelStream = new FileStream(viewModelPath, FileMode.Open))
            using (FileStream modelStream = new FileStream(modelPath, FileMode.Open))
            using (XmlReader viewModelReader = XmlTextReader.Create(viewModelStream, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.None, XmlResolver = null }))
            using (XmlReader modelReader = XmlReader.Create(modelStream))
            using (XmlWriter viewWriter = XmlWriter.Create(viewPath,BindTransform.OutputSettings))
            {
                return Bind(log: log, modelReader: modelReader, viewModelReader: viewModelReader, viewWriter: viewWriter, messageAction: messageAction);
            }
        }

        public static string Bind(TaskLoggingHelper log, string modelXml, string viewModelXml, Action<object, XsltMessageEncounteredEventArgs> messageAction = null)
        {
            using (XmlReader viewModelReader = XmlTextReader.Create(new StringReader(viewModelXml), new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse, ValidationType = ValidationType.None, XmlResolver = null }))
            using (XmlReader modelReader = XmlReader.Create(new StringReader(modelXml)))
            using (StringWriter viewDocumentWriter = new StringWriter())
            using (XmlWriter viewWriter = XmlWriter.Create(viewDocumentWriter, BindTransform.OutputSettings))
            {
                Bind(log:log, modelReader: modelReader, viewModelReader: viewModelReader, viewWriter: viewWriter, messageAction:messageAction);
                return viewDocumentWriter.ToString();
            }
        }
    }
}
