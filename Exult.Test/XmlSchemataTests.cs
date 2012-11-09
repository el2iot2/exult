using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Schema;

namespace Exult.Test
{
    [TestClass]
    public class XmlSchemataTests
    {
        [TestMethod]
        public void LoadAndValidate_ItemsSchema()
        {
            XmlSchema schema = XmlSchemata.ItemsSchema;
        }

        //[TestMethod]
        //public void LoadAndValidate_BindingsSchema()
        //{
        //    XmlSchema schema = XmlSchemata.BindingsSchema;
        //}
        
        [TestMethod]
        public void LoadAndValidate_XhtmlFramesetSchema()
        {
            XmlSchema schema = XmlSchemata.XhtmlFramesetSchema;
        }

        [TestMethod]
        public void LoadAndValidate_XhtmlStrictSchema()
        {
            XmlSchema schema = XmlSchemata.XhtmlStrictSchema;
        }

        [TestMethod]
        public void LoadAndValidate_XhtmlTransitionalSchema()
        {
            XmlSchema schema = XmlSchemata.XhtmlTransitionalSchema;
        }

        [TestMethod]
        public void LoadAndValidate_XmlSchema()
        {
            XmlSchema schema = XmlSchemata.XmlSchema;
        }
    }
}
