using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Lawllit.Web.Common;

// A chave vem pronta da variável de ambiente. Volume na Railway monta como root, e o contêiner roda sem root.
public sealed class ChaveFixaRepository(string xml) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements() => [XElement.Parse(xml)];

    // A geração automática fica desligada, então chegar aqui é erro de configuração e não pode passar calado.
    public void StoreElement(XElement element, string friendlyName)
        => throw new InvalidOperationException("A chave de Data Protection é fixa. Gere uma nova e troque DataProtection__Chave.");
}
