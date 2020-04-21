using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// This Solr schema populate helper uses the built-in one, but adds logging of
    /// what is actually sent to Solr before sending it.
    /// </summary>
    public class DefaultWithDebugPopulateHelper : Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema.SchemaPopulateHelper
    {
        public DefaultWithDebugPopulateHelper(SolrSchema solrSchema) : base(solrSchema)
        {
        }

        public override IEnumerable<XElement> GetAllFields()
        {
            var fields = base.GetAllFields().ToList();
            ConfigurationDrivenPopulateHelper.LogSolrJsonUpdateCommand(fields);
            return fields;
        }

        public override IEnumerable<XElement> GetAllFieldTypes()
        {
            var fieldTypes = base.GetAllFieldTypes().ToList();
            ConfigurationDrivenPopulateHelper.LogSolrJsonUpdateCommand(fieldTypes);
            return fieldTypes;
        }
    }
}
