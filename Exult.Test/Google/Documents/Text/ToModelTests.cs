using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Schema;

namespace Exult.Test.Google.Documents
{
[TestClass]
public class ToModelTests
{
[TestMethod]
public void ModelSimpleHtml()
{
string html =
@"<html><body>body</body></html>";

string expectedModel =
"<xm:document xmlns:xm=\"http://code.google.com/p/exult/model\" xmlns:xb=\"http://code.google.com/p/exult/binding\">" + @"
"+"  <div xb:class=\"body\">body</div>"+@"
</xm:document>";

ToModel(html, expectedModel);
}

[TestMethod]
public void ModelUppercaseHtml()
{
    string html =
    @"<HTML><BODY>BODY</BODY></HTML>";

    string expectedModel =
    "<xm:document xmlns:xm=\"http://code.google.com/p/exult/model\" xmlns:xb=\"http://code.google.com/p/exult/binding\">" + @"
" + "  <div xb:class=\"body\">BODY</div>" + @"
</xm:document>";

    ToModel(html, expectedModel);
}

[TestMethod]
public void ModelMixedcaseHtml()
{
    string html =
    @"<HTML><BoDy>BoDy</BoDy></HTML>";

    string expectedModel =
    "<xm:document xmlns:xm=\"http://code.google.com/p/exult/model\" xmlns:xb=\"http://code.google.com/p/exult/binding\">" + @"
" + "  <div xb:class=\"body\">BoDy</div>" + @"
</xm:document>";

    ToModel(html, expectedModel);
}


public void ToModel(string html, string expectedModel, IDictionary<string, object> metadata = null)
{
    string model = XslTransforms.Google.Documents.Text.ToModel(html, metadata, messageAction: (o, a) => 
{
    Console.WriteLine(a.ToString());
});
Assert.AreEqual(expectedModel, model);
}
}
}
