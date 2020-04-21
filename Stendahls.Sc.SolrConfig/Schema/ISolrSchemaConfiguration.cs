using System.Collections.Generic;
using JetBrains.Annotations;

namespace Stendahls.Sc.SolrConfig.Schema
{
    public interface ISolrSchemaConfiguration
    {
        bool RemoveAllFields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaField> Fields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaDynamicField> DynamicFields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaFieldType> FieldTypes { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaCopyField> CopyFields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaField> RemoveFields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaDynamicField> RemoveDynamicFields { get; }

        [NotNull, ItemNotNull]
        IEnumerable<SolrSchemaCopyField> RemoveCopyFields { get; }

        /// <summary>
        /// Validates the configured schema prior sending it to Solr
        /// </summary>
        /// <returns></returns>
        bool IsValid();
    }
}
