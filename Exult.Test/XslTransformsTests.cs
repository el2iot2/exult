using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace Exult.Test
{
    [TestClass]
    public class XslSchemataTests
    {
        [TestMethod]
        public void LoadAndValidate_BindTransform()
        {
            XslCompiledTransform transform = XslTransforms.BindTransform;
        }

        [TestMethod]
        public void LoadAndValidate_Google_Documents_ModelTextDocumentTransform()
        {
            XslCompiledTransform transform = XslTransforms.Google.Documents.Text.ToModelTransform;
        }
    }
}
