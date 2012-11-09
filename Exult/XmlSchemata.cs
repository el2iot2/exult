using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace Exult
{
    public static class XmlSchemata
    {
        static XmlSchema _Items = null;
        public static XmlSchema ItemsSchema
        { 
            get 
            {
                return _Items = _Items ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Items.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        static XmlSchema _Bindings = null;
        public static XmlSchema BindingsSchema
        {
            get
            {
                return _Bindings = _Bindings ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Bindings.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        static XmlSchema _Xhtml_Frameset = null;
        public static XmlSchema XhtmlFramesetSchema
        {
            get
            {
                return _Xhtml_Frameset = _Xhtml_Frameset ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Standard.xhtml1-frameset.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        static XmlSchema _Xhtml_Strict = null;
        public static XmlSchema XhtmlStrictSchema
        {
            get
            {
                return _Xhtml_Strict = _Xhtml_Strict ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Standard.xhtml1-strict.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        static XmlSchema _Xhtml_Transitional = null;
        public static XmlSchema XhtmlTransitionalSchema
        {
            get
            {
                return _Xhtml_Transitional = _Xhtml_Transitional ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Standard.xhtml1-transitional.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        static XmlSchema _Xml = null;
        public static XmlSchema XmlSchema
        {
            get
            {
                return _Xml = _Xml ?? XmlSchemata.LoadSchemaResource("Exult.Schema.Standard.xml.xsd", (Object o, ValidationEventArgs a) => { throw a.Exception; });
            }
        }

        
        public static bool Validate(this XmlSchemaSet schemaSet, string documentPath, Action<object, ValidationEventArgs> validationAction=null)
        {
            validationAction = validationAction ?? new Action<object, ValidationEventArgs>((o, a) => { });
            XmlDocument d = new XmlDocument();
            d.Schemas = schemaSet;
            d.Load(documentPath);
            bool errorFound = false;

            d.Validate(new ValidationEventHandler(
                delegate(Object sender, ValidationEventArgs e)
                {
                    validationAction(sender, e);
                    errorFound |= e.Exception != null;
                }
            ));
            return !errorFound;
        }

        public static XmlSchema LoadSchema(XmlReader schemaReader, Action<object, ValidationEventArgs> validationAction=null)
        {
            validationAction = validationAction ?? new Action<object, ValidationEventArgs>((o, a) => { });
            return XmlSchema.Read(schemaReader, new ValidationEventHandler(
            delegate(Object sender, ValidationEventArgs e)
            {
                validationAction(sender, e);
            }
            ));
        }

        public static XmlSchema LoadSchemaResource(string name, Action<object, ValidationEventArgs> validationAction = null)
        {
            using (XmlReader schemaReader = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
            {
                return LoadSchema(schemaReader, validationAction:validationAction);
            }
        }

    }
}
