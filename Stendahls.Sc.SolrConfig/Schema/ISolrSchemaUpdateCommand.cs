using System.Xml.Linq;
using JetBrains.Annotations;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    public interface ISolrSchemaUpdateCommand
    {
        [CanBeNull]
        XElement SerializeUpdateCommand([NotNull]SolrSchema schema);
    }
}
