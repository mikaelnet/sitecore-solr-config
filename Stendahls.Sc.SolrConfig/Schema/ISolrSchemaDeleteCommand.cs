using System.Xml.Linq;
using JetBrains.Annotations;
using SolrNet.Schema;

namespace Stendahls.Sc.SolrConfig.Schema
{
    public interface ISolrSchemaDeleteCommand
    {
        [CanBeNull]
        XElement SerializeDeleteCommand([NotNull]SolrSchema schema);
    }
}
