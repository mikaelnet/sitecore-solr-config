using System.Collections.Generic;
using JetBrains.Annotations;
using Sitecore.ContentSearch.SolrProvider.Abstractions;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// Replaces the Sitecore DefaultPopulateHelperFactory from the config, so
    /// that we can replace the built-in SchemaPopulateHelper
    /// </summary>
    [UsedImplicitly]
    public class ConfigurationDrivenPopulateHelperFactory : IPopulateHelperFactory
    {
        public ISchemaPopulateHelper GetPopulateHelper(SolrSchema solrSchema)
        {
            // Sitecore default implementation returns SchemaPopulateHelper
            // Optionally, DefaultWithDebugPopulateHelper can be returned using
            // Sitecore's default implementation but adds logging.
            return new ConfigurationDrivenPopulateHelper(solrSchema);
        }
    }
}
