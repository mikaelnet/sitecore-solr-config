using System.Xml.Linq;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// Defines a Solr schema add or replace dynamic field command.
    /// https://lucene.apache.org/solr/guide/6_6/schema-api.html#SchemaAPI-AddaDynamicFieldRule
    /// </summary>
    public class SolrSchemaDynamicField : SolrSchemaField
    {
        public SolrSchemaDynamicField()
        {
        }

        public SolrSchemaDynamicField(string name) : base(name)
        {
        }

        protected override string GetOperation(SolrSchema schema)
        {
            return schema.SolrDynamicFields.Find(f => f.Name == Name) != null ? 
                "replace-dynamic-field" : "add-dynamic-field";
        }

        public override XElement SerializeDeleteCommand(SolrSchema schema)
        {
            var existingField = schema.SolrDynamicFields.Find(f => f.Name == Name);
            if (existingField == null)
                return null;

            XElement deleteField = new XElement("delete-dynamic-field");
            deleteField.Add(new XElement("name", Name));
            return deleteField;
        }
    }
}
