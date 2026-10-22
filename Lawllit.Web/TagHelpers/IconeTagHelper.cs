using Lawllit.Web.Common;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Lawllit.Web.TagHelpers;

// <icone nome="lupa" /> vira SVG inline, sem fonte de ícone para baixar. Decorativo por padrão.
[HtmlTargetElement("icone", TagStructure = TagStructure.WithoutEndTag)]
public sealed class IconeTagHelper : TagHelper
{
    public string Nome { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Icones.Caminhos.TryGetValue(Nome, out var caminho))
            throw new InvalidOperationException($"O ícone '{Nome}' não existe em Icones.Caminhos.");

        var classeExtra = output.Attributes["class"]?.Value?.ToString();

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", string.IsNullOrWhiteSpace(classeExtra) ? "icone" : $"icone {classeExtra}");
        output.Attributes.SetAttribute("viewBox", "0 0 24 24");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("focusable", "false");
        output.Content.SetHtmlContent($"<path d=\"{caminho}\"></path>");
    }
}
