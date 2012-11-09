using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Schema;
using Microsoft.Build.Utilities;

namespace Exult.Test
{
[TestClass]
public class BindTests
{
[TestMethod]
public void BindDotToAttribute()
{
string viewModelXml =
@"<title id='.=>@title'>
A Default Title
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
@"My Specialized Title";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindDotToAttribute_Default()
{
string viewModelXml =
@"<title id='=>@title'>
A Default Title
</title>";

string modelXml =
@"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
@"My Specialized Title";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindDotToElement()
{
string viewModelXml =
@"<title id='.=>#title'>
A Default Title
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><section xb:id='title'>text</section></xm:document>";

string expectedViewXml =
@"<section>text</section>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindDotToClass()
{
string viewModelXml =
@"<title id='.=>.title'>
A Default Title
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><a xb:class='title'>a</a><b xb:class='title'>b</b><c xb:class='title'>c</c></xm:document>";

string expectedViewXml =
@"<a>a</a>
<b>b</b>
<c>c</c>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToAttribute()
{
string viewModelXml =
@"<title id='*=>@title'>
<a>something</a>
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' title='other'/>";

string expectedViewXml =
@"<title>other</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToAttribute_ResidualId()
{
string viewModelXml =
@"<title id='*=>@title:myid'>
<a>something</a>
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' title='other'/>";

string expectedViewXml =
"<title id=\"myid\">other</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToElement()
{
string viewModelXml =
@"<title id='*=>#title'><a>something</a></title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><section xb:id='title'>text</section></xm:document>";

string expectedViewXml =
@"<title>
  <section>text</section>
</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToElement_ResidualId()
{
string viewModelXml =
@"<title id='*=>#title:myid'>
<a>something</a>
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><section xb:id='title'>text</section></xm:document>";

string expectedViewXml =
"<title id=\"myid\">"+@"
  <section>text</section>
</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToClass()
{
string viewModelXml =
@"<title id='*=>.title'><a>something</a></title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><a xb:class='title'>a</a><b xb:class='title'>b</b><c xb:class='title'>c</c></xm:document>";

string expectedViewXml =
@"<title>
  <a>a</a>
  <b>b</b>
  <c>c</c>
</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindStarToClass_ResidualId()
{
string viewModelXml =
@"<title id='*=>.title:myid'>
<a>something</a>
</title>";

string modelXml =
@"<xm:document xmlns:xm='http://code.google.com/p/exult/model' xmlns:xb='http://code.google.com/p/exult/binding'><a xb:class='title'>a</a><b xb:class='title'>b</b><c xb:class='title'>c</c></xm:document>";

string expectedViewXml =
"<title id=\"myid\">" + @"
  <a>a</a>
  <b>b</b>
  <c>c</c>
</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute()
{
string viewModelXml =
@"<title id='text()=>@title'>
A Default Title
</title>";

string modelXml =
@"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
@"<title>My Specialized Title</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute_ResidualId()
{
string viewModelXml =
@"<title id='text()=>@title:titleid'>
A Default Title
</title>";

string modelXml =
@"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
"<title id=\"titleid\">My Specialized Title</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindAttributeToAttribute()
{
    string viewModelXml =
    @"<title text='=>@title' />";

    string modelXml =
    @"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

    string expectedViewXml =
    "<title text=\"My Specialized Title\" />";

    Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindAttributeToInlineSubstitutionAttribute()
{
    string viewModelXml =
    @"<tag text='=>Prefix.@{attr}.Suffix' />";

    string modelXml =
    @"<document xmlns='http://code.google.com/p/exult/model' attr='Attr' />";

    string expectedViewXml =
    "<tag text=\"Prefix.Attr.Suffix\" />";

    Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindAttributeToPrefixSubstitutionAttribute()
{
    string viewModelXml =
    @"<tag text='=>@{attr}.Suffix' />";

    string modelXml =
    @"<document xmlns='http://code.google.com/p/exult/model' attr='Attr' />";

    string expectedViewXml =
    "<tag text=\"Attr.Suffix\" />";

    Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindAttributeToSuffixSubstitutionAttribute()
{
    string viewModelXml =
    @"<tag text='=>Prefix.@{attr}' />";

    string modelXml =
    @"<document xmlns='http://code.google.com/p/exult/model' attr='Attr' />";

    string expectedViewXml =
    "<tag text=\"Prefix.Attr\" />";

    Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute_Document_Wrapper()
{
string viewModelXml =
@"<xvm:document xmlns:xvm='http://code.google.com/p/exult/viewmodel'>
<title id='text()=>@title'>
A Default Title
</title>
</xvm:document>";

string modelXml =
@"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
"<title>My Specialized Title</title>";
Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute_Preamble_XmlDeclaration()
{
string viewModelXml =
@"<xvm:document xmlns:xvm='http://code.google.com/p/exult/viewmodel'>
<xvm:preamble>
<![CDATA[<?xml version='1.0' encoding='utf-8'?>]]>
</xvm:preamble>
<title id='text()=>@title'>
A Default Title
</title>
</xvm:document>";

string modelXml =
@"<document xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
"<?xml version='1.0' encoding='utf-8'?><title>My Specialized Title</title>";
Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute_Preamble_DocType()
{
string viewModelXml =
@"<xvm:document xmlns:xvm='http://code.google.com/p/exult/viewmodel'>
<xvm:preamble>
<![CDATA[<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>
]]>
</xvm:preamble>
<title id='text()=>@title'>A Default Title</title>
</xvm:document>";

string modelXml =
@"<xm:model xmlns:xm='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
@"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>
<title>My Specialized Title</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

[TestMethod]
public void BindTextToAttribute_Preamble_DocType_XmlDeclaration()
{
string viewModelXml =
@"<xvm:document xmlns:xvm='http://code.google.com/p/exult/viewmodel'>
<xvm:preamble>
<![CDATA[<?xml version='1.0' encoding='utf-8'?>
<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>
]]>
</xvm:preamble>
<title id='text()=>@title'>A Default Title</title>
</xvm:document>";

string modelXml =
@"<model xmlns='http://code.google.com/p/exult/model' title='My Specialized Title' />";

string expectedViewXml =
@"<?xml version='1.0' encoding='utf-8'?>
<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>
<title>My Specialized Title</title>";

Bind(modelXml, viewModelXml, expectedViewXml);
}

public void Bind(string modelXml, string viewModelXml, string expectedViewXml)
{
string viewXml = XslTransforms.Bind(new TestTaskLoggingHelper(), modelXml, viewModelXml, messageAction: (o, a) => 
{
    Console.WriteLine(a.ToString());
});
Assert.AreEqual(expectedViewXml, viewXml);
}
}
}
