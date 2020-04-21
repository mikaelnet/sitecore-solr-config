using System.Xml.Linq;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    public class SolrSchemaCopyField : ISolrSchemaUpdateCommand, ISolrSchemaDeleteCommand
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public int MaxChars { get; set; }

        public virtual XElement SerializeUpdateCommand(SolrSchema schema)
        {
            var copyField = new XElement("add-copy-field");

            copyField.Add(new XElement("source", Source));
            copyField.Add(new XElement("dest", Destination));
            if (MaxChars > 0)
                copyField.Add(new XElement("maxChars", MaxChars));
            
            return copyField;
        }

        public XElement SerializeDeleteCommand(SolrSchema schema)
        {
            if (schema.SolrCopyFields.Find(f => f.Source == Source && f.Destination == Destination) == null)
                return null;

            XElement deleteField = new XElement("delete-copy-field");
            deleteField.Add(new XElement("source", Source));
            deleteField.Add(new XElement("dest", Destination));
            return deleteField;
        }
    }
}
