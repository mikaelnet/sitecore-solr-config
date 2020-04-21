using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using Sitecore.Diagnostics;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// Defines a Solr schema add or replace field type command.
    /// https://lucene.apache.org/solr/guide/6_6/schema-api.html#SchemaAPI-AddaNewFieldType
    /// </summary>
    public class SolrSchemaFieldType : ISolrSchemaUpdateCommand
    {
        public string Name { get; set; }

        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        private readonly List<XmlElement> _analyzers = new List<XmlElement>();

        internal void AddAnalyzer([NotNull]XmlElement analyzer)
        {
            _analyzers.Add(analyzer);
        }

        public virtual XElement SerializeUpdateCommand(SolrSchema schema)
        {
            var existingFieldType = schema.SolrFieldTypes.Find(f => f.Name == Name);
            var fieldType = new XElement(existingFieldType == null ? "add-field-type" : "replace-field-type");
            
            fieldType.Add(new XElement("name", Name));

            foreach (var pair in Properties)
            {
                fieldType.Add(new XElement(pair.Key, pair.Value));
            }
            
            // There can be an index + query analyzer, one common untyped analyzer or none at all.
            var indexAnalyzer = _analyzers.SingleOrDefault(a => a.Attributes["type"]?.Value == "index");
            var queryAnalyzer = _analyzers.SingleOrDefault(a => a.Attributes["type"]?.Value == "query");
            var genericAnalyzer = _analyzers.SingleOrDefault(a => !a.HasAttribute("type"));

            if (indexAnalyzer != null && queryAnalyzer != null)
            {
                fieldType.Add(SerializeAnalyzer(indexAnalyzer));
                fieldType.Add(SerializeAnalyzer(queryAnalyzer));
            }
            else if (genericAnalyzer != null)
            {
                fieldType.Add(SerializeAnalyzer(genericAnalyzer));
            }

            return fieldType;
        }

        protected virtual XElement SerializeAnalyzer(XmlElement analyzerElement)
        {
            string analyzerTypeName;
            switch(analyzerElement.GetAttribute("type"))
            {
                case "index":
                    analyzerTypeName = "indexAnalyzer";
                    break;
                case "query":
                    analyzerTypeName = "queryAnalyzer";
                    break;
                default:
                    analyzerTypeName = "analyzer";
                    break;
            }
            XElement analyzer = new XElement(analyzerTypeName);

            var charFilterConfig = analyzerElement.SelectNodes("charFilter");
            if (charFilterConfig != null)
            {
                int numberOfCharFilters = 0;
                foreach (XmlElement charFilter in charFilterConfig)
                {
                    var charFilterStage = new XElement("charFilters");
                    foreach (XmlAttribute attribute in charFilter.Attributes)
                    {
                        charFilterStage.Add(new XElement(attribute.Name, attribute.Value));
                    }
                    analyzer.Add(charFilterStage);
                    numberOfCharFilters++;
                }

                if (numberOfCharFilters == 1)
                {
                    Log.Warn("Sitecore has a design flaw that could make a single charFilter not work as expected", nameof(SolrSchemaFieldType));
                }
            }

            var tokenizerConfig = analyzerElement.SelectSingleNode("tokenizer");
            if (tokenizerConfig?.Attributes == null)
                throw new ConfigurationErrorsException("FieldType missing mandatory tokenizer");

            var tokenizerStage = new XElement("tokenizer");
            foreach (XmlAttribute attribute in tokenizerConfig.Attributes)
            {
                tokenizerStage.Add(new XElement(attribute.Name, attribute.Value));
            }
            analyzer.Add(tokenizerStage);

            var filters = analyzerElement.SelectNodes("filter");
            if (filters != null)
            {
                foreach (XmlElement filter in filters)
                {
                    XElement filterStage = new XElement("filters");
                    foreach (XmlAttribute attribute in filter.Attributes)
                    {
                        filterStage.Add(new XElement(attribute.Name, attribute.Value));
                    }
                    analyzer.Add(filterStage);
                }
            }

            return analyzer;
        }
    }
}
