using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Schema;

namespace Exult.Test
{
    [TestClass]
    public class TerrafirmaTests
    {
        [TestMethod]
        public void LoadAndValidate_ItemsSchema()
        {

            XmlSchema schema = XmlSchemata.ItemsSchema;
        }
    }
}
