using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;

namespace Stendahls.Sc.SolrConfig.Schema
{
    public class SolrSchemaConfiguration : ISolrSchemaConfiguration
    {
        public bool RemoveAllFields { get; [UsedImplicitly] set; }

        private readonly List<SolrSchemaField> _solrSchemaFields = new List<SolrSchemaField>();
        public IEnumerable<SolrSchemaField> Fields => _solrSchemaFields;

        private readonly List<SolrSchemaDynamicField> _solrSchemaDynamicFields = new List<SolrSchemaDynamicField>();
        public IEnumerable<SolrSchemaDynamicField> DynamicFields => _solrSchemaDynamicFields;

        private readonly List<SolrSchemaFieldType> _solrSchemaFieldsTypes = new List<SolrSchemaFieldType>();
        public IEnumerable<SolrSchemaFieldType> FieldTypes => _solrSchemaFieldsTypes;

        private readonly List<SolrSchemaCopyField> _solrSchemaCopyFields = new List<SolrSchemaCopyField>();
        public IEnumerable<SolrSchemaCopyField> CopyFields => _solrSchemaCopyFields;

        private readonly List<SolrSchemaField> _removeFields = new List<SolrSchemaField>();
        public IEnumerable<SolrSchemaField> RemoveFields => _removeFields;

        private readonly List<SolrSchemaDynamicField> _removeDynamicFields = new List<SolrSchemaDynamicField>();
        public IEnumerable<SolrSchemaDynamicField> RemoveDynamicFields => _removeDynamicFields;

        private readonly List<SolrSchemaCopyField> _removeCopyFields = new List<SolrSchemaCopyField>();
        public IEnumerable<SolrSchemaCopyField> RemoveCopyFields => _removeCopyFields;

        public bool IsValid()
        {
            if (RemoveFields.Any(r => Fields.Any(f => f.Name == r.Name)))
            {
                Sitecore.Diagnostics.Log.Error("You can't both add and remove the same field", nameof(SolrSchemaConfiguration));
                return false;
            }

            if (RemoveDynamicFields.Any(r => DynamicFields.Any(f => f.Name == r.Name)))
            {
                Sitecore.Diagnostics.Log.Error("You can't both add and remove the same dynamic field", nameof(SolrSchemaConfiguration));
                return false;
            }

            // Potentially add more validation rules here

            return true;
        }

        /// <summary>
        /// Adds a field to the managed schema:
        /// &lt;fields hint="raw:AddField"&gt;
        ///   &lt;field name="nnn" type="nnn" ... /&gt;
        /// </summary>
        /// <param name="node"></param>
        [UsedImplicitly]
        protected void AddField([CanBeNull]XmlNode node)
        {
            if (node?.Attributes?["name"] == null)
                return;

            var field = new SolrSchemaField();
            SetProperties(node, field);
            _solrSchemaFields.Add(field);
        }

        /// <summary>
        /// Adds a dynamic field to the managed schema:
        /// &lt;dynamicFields hint="raw:AddDynamicField"&gt;
        ///   &lt;dynamicField name="*_nnn" type="nnn" ... /&gt;
        /// </summary>
        /// <param name="node"></param>
        [UsedImplicitly]
        protected void AddDynamicField([CanBeNull]XmlNode node)
        {
            if (node?.Attributes?["name"] == null)
                return;

            var dynamicField = new SolrSchemaDynamicField();
            SetProperties(node, dynamicField);
            _solrSchemaDynamicFields.Add(dynamicField);
        }

        /// <summary>
        /// Adds a field type definition to the managed schema:
        /// &lt;fieldTypes hint="raw:AddFieldType"&gt;
        ///   &lt;fieldType name="type" class="nnn" ...
        ///     &lt;analyzer ...
        /// </summary>
        /// <param name="node"></param>
        [UsedImplicitly]
        protected void AddFieldType([CanBeNull]XmlNode node)
        {
            if (node?.Attributes?["name"] == null || node.Attributes?["class"] == null)
                return;

            var fieldType = new SolrSchemaFieldType
            {
                Name = GetStringParameter(node, "name"), 
            };

            foreach (XmlAttribute attr in node.Attributes)
            {
                // skip all attributes from other namespaces, such as Sitecore's patch:source="..."
                if (attr.Name == "name" || attr.Name.Contains(":"))
                    continue;

                fieldType.Properties.Add(attr.Name, attr.Value);
            }

            var analyzers = node.SelectNodes("analyzer");
            if (analyzers != null)
            {
                foreach (XmlElement analyzer in analyzers)
                {
                    fieldType.AddAnalyzer(analyzer);
                }
            }

            _solrSchemaFieldsTypes.Add(fieldType);
        }

        /// <summary>
        /// Adds a copy field definition to the managed schema:
        /// &lt;copyFields hint="raw:AddCopyField"&gt;
        /// &lt;copyField source="nnn" dest="nnn" /&gt;
        /// </summary>
        /// <param name="node"></param>
        [UsedImplicitly]
        protected void AddCopyField([CanBeNull]XmlNode node)
        {
            var copyField = ParseCopyFieldNode(node);
            if (copyField != null)
                _solrSchemaCopyFields.Add(copyField);
        }

        [UsedImplicitly]
        protected void RemoveField([CanBeNull]XmlNode node)
        {
            var name = GetStringParameter(node, "name");
            if (!string.IsNullOrWhiteSpace(name))
                _removeFields.Add(new SolrSchemaField(name));
        }

        [UsedImplicitly]
        protected void RemoveDynamicField([CanBeNull]XmlNode node)
        {
            var name = GetStringParameter(node, "name");
            if (!string.IsNullOrWhiteSpace(name))
                _removeDynamicFields.Add(new SolrSchemaDynamicField(name));
        }

        [UsedImplicitly]
        protected void RemoveCopyField([CanBeNull]XmlNode node)
        {
            var copyField = ParseCopyFieldNode(node);
            if (copyField != null)
                _removeCopyFields.Add(copyField);
        }

        private void SetProperties ([NotNull]XmlNode node, [NotNull]SolrSchemaField field)
        {
            if (node.Attributes?["type"] == null)
                return;

            field.Name = GetStringParameter(node, "name");
            foreach (XmlAttribute attr in node.Attributes)
            {
                // skip all attributes from other namespaces, such as Sitecore's patch:source="..."
                if (attr.Name == "name" || attr.Name.Contains(":"))
                    continue;

                field.Properties.Add(attr.Name, attr.Value);
            }
        }

        [CanBeNull]
        private SolrSchemaCopyField ParseCopyFieldNode([CanBeNull]XmlNode node)
        {
            if (node?.Attributes?["source"] == null || node.Attributes?["dest"] == null)
                return null;

            var copyField = new SolrSchemaCopyField
            {
                Source = GetStringParameter(node, "source"),
                Destination = GetStringParameter(node, "dest"),
                MaxChars = GetIntegerParameter(node, "maxChars")
            };
            return copyField;
        }

        [CanBeNull]
        private string GetStringParameter([CanBeNull]XmlNode node, [NotNull]string name, [CanBeNull]string @default = null)
        {
            return node?.Attributes?[name]?.Value ?? @default;
        }

        private int GetIntegerParameter([CanBeNull]XmlNode node, [NotNull]string name, int @default = 0)
        {
            var value = GetStringParameter(node, name);

            if (string.IsNullOrWhiteSpace(value))
                return @default;

            if (int.TryParse(value, out var result))
                return result;

            return @default;
        }
    }
}
