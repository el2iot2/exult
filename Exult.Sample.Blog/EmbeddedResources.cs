using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace Exult.Sample.Blog
{
    public static class EmbeddedResources
    {
        public static XmlReader TerrafirmaIndexHtml { get { return new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Exult.Sample.Blog.terrafirma.index.html")); } }
    }
}
