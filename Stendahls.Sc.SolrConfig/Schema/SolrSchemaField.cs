using System.Collections.Generic;
using System.Xml.Linq;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// Defines a Solr schema add or replace field command.
    /// https://lucene.apache.org/solr/guide/6_6/schema-api.html#SchemaAPI-AddaNewField
    /// </summary>
    public class SolrSchemaField : ISolrSchemaUpdateCommand, ISolrSchemaDeleteCommand
    {
        public string Name { get; set; }
        public IDictionary<string,string> Properties { get; } = new Dictionary<string, string>();

        public SolrSchemaField()
        {
        }

        public SolrSchemaField(string name)
        {
            Name = name;
        }

        protected virtual string GetOperation(SolrSchema schema)
        {
            return schema.SolrFields.Find(f => f.Name == Name) != null ? 
                "replace-field" : "add-field";
        }

        public virtual XElement SerializeUpdateCommand(SolrSchema schema)
        {
            XElement field = new XElement(GetOperation(schema));
            field.Add(new XElement("name", Name));

            foreach (var pair in Properties)
            {
                field.Add(new XElement(pair.Key, pair.Value));
            }

            return field;
        }

        public virtual XElement SerializeDeleteCommand(SolrSchema schema)
        {
            if (schema.SolrFields.Find(f => f.Name == Name) == null)
                return null;

            XElement deleteField = new XElement("delete-field");
            deleteField.Add(new XElement("name", Name));
            return deleteField;
        }
    }
}
