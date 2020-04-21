using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sitecore.ContentSearch.SolrProvider.Pipelines.PopulateSolrSchema;
using Sitecore.Diagnostics;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    /// <summary>
    /// This SchemaPopulateHelper implementation replaces the Sitecore built-in
    /// helper by picking the Solr managed schema configuration from a Sitecore
    /// config instead of having it hard coded.
    /// </summary>
    public class ConfigurationDrivenPopulateHelper : ISchemaPopulateHelper
    {
        private readonly SolrSchema _solrSchema;
        private readonly ISolrSchemaConfiguration _solrSolrSchemaConfiguration;

        public ConfigurationDrivenPopulateHelper([NotNull]SolrSchema solrSchema)
        {
            _solrSchema = solrSchema;

            var xmlNode = Sitecore.Configuration.Factory.GetConfigNode("contentSearch/indexConfigurations/solrManagedSchema");
            _solrSolrSchemaConfiguration = Sitecore.Configuration.Factory.CreateObject<ISolrSchemaConfiguration>(xmlNode);

            if (!_solrSolrSchemaConfiguration.IsValid())
            {
                Log.Error("Solr Schema configuration errors found. Schema population may fail.", nameof(ConfigurationDrivenPopulateHelper));
            }
        }

        [NotNull, ItemNotNull]
        public IEnumerable<XElement> GetAllFields()
        {
            var allFields = new List<XElement>();
            foreach (var removeField in GetRemoveFields())
            {
                allFields.Add(removeField);
            }

            foreach (var field in _solrSolrSchemaConfiguration.Fields)
            {
                allFields.Add(field.SerializeUpdateCommand(_solrSchema));
            }

            foreach (var dynamicField in _solrSolrSchemaConfiguration.DynamicFields)
            {
                allFields.Add(dynamicField.SerializeUpdateCommand(_solrSchema));
            }

            foreach (var copyField in _solrSolrSchemaConfiguration.CopyFields)
            {
                allFields.Add(copyField.SerializeUpdateCommand(_solrSchema));
            }

            LogSolrJsonUpdateCommand(allFields);
            return allFields.Where(e => e != null);
        }

        [NotNull, ItemNotNull]
        public IEnumerable<XElement> GetAllFieldTypes()
        {
            var allFieldTypes = new List<XElement>();

            foreach (var fieldType in _solrSolrSchemaConfiguration.FieldTypes)
            {
                allFieldTypes.Add(fieldType.SerializeUpdateCommand(_solrSchema));
            }

            LogSolrJsonUpdateCommand(allFieldTypes);
            return allFieldTypes.Where(e => e != null);
        }

        /// <summary>
        /// Creates the solr "remove" commands to remove surplus fields and avoid
        /// duplicates of copy-fields etc that cannot be updated. Sitecore default
        /// implementation always removes all fields and re-creates them. This
        /// implementation has remove all fields as an option, so that unmanaged
        /// fields can be left untouched.
        /// </summary>
        /// <returns></returns>
        [NotNull, ItemCanBeNull]
        public IEnumerable<XElement> GetRemoveFields()
        {
            return _solrSolrSchemaConfiguration.RemoveAllFields ? GetRemoveAllFields() : GetRemoveSurplusFields();
        }

        /// <summary>
        /// Returns remove commands for all existing fields, equal to the default
        /// Sitecore implementation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XElement> GetRemoveAllFields()
        {
            foreach (var solrCopyField in _solrSchema.SolrCopyFields)
            {
                var d = new SolrSchemaCopyField { Source = solrCopyField.Source, Destination = solrCopyField.Destination };
                yield return d.SerializeDeleteCommand(_solrSchema);
            }

            foreach (var solrDynamicField in _solrSchema.SolrDynamicFields
                .Where(s => _solrSolrSchemaConfiguration.DynamicFields.All(f => f.Name != s.Name)))
            {
                var d = new SolrSchemaDynamicField(solrDynamicField.Name);
                yield return d.SerializeDeleteCommand(_solrSchema);
            }

            foreach (var solrField in _solrSchema.SolrFields
                .Where(s => _solrSolrSchemaConfiguration.Fields.All(f => f.Name != s.Name)))
            {
                var d = new SolrSchemaField(solrField.Name);
                yield return d.SerializeDeleteCommand(_solrSchema);
            }
        }

        /// <summary>
        /// Returns remove commands for only specified fields. Used when
        /// removeAllFields is false.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XElement> GetRemoveSurplusFields()
        {
            // Note: There is no way to update a copy-field and adding a copy field
            // twice creates duplicates. Therefore all copy-fields always has to be
            // removed before re-adding them.
            foreach (var field in _solrSolrSchemaConfiguration.RemoveCopyFields)
            {
                yield return field.SerializeDeleteCommand(_solrSchema);
            }

            foreach (var solrCopyField in _solrSchema.SolrCopyFields)   // Those will be re-added later
            {
                var d = new SolrSchemaCopyField { Source = solrCopyField.Source, Destination = solrCopyField.Destination };
                yield return d.SerializeDeleteCommand(_solrSchema);
            }

            foreach (var field in _solrSolrSchemaConfiguration.RemoveDynamicFields)
            {
                yield return field.SerializeDeleteCommand(_solrSchema);
            }

            foreach (var field in _solrSolrSchemaConfiguration.RemoveFields)
            {
                yield return field.SerializeDeleteCommand(_solrSchema);
            }
        }

        /// <summary>
        /// Internally, Sitecore creates a Solr update command from the Xml structure.
        /// This method constructs an json command as Sitecore does and writes it
        /// to the Sitecore log for debugging purposes.
        /// </summary>
        /// <param name="elements"></param>
        internal static void LogSolrJsonUpdateCommand([ItemCanBeNull] IEnumerable<XElement> elements)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            foreach (XElement element in elements.Where(e => e != null))
            {
                if (!element.IsEmpty)
                {
                    string str = JsonConvert.SerializeXNode(element);
                    sb.Append(str.Substring(1, str.Length - 2));
                    sb.Append(",");
                }
            }

            // Replace trailing ',' with '}' to complete the json format
            sb[sb.Length - 1] = '}';

            Log.Info($"Solr Schema update:\r\n{sb}", nameof(ConfigurationDrivenPopulateHelper));
        }
    }
}
