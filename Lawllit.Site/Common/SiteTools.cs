namespace Lawllit.Site.Common;

// As ferramentas, num lugar só. Antes cada card estava escrito à mão na Views/Tools/
// Index.cshtml, e a home nova precisa da mesma lista para o card de projetos, o que
// duplicaria as onze entradas.
public static class SiteTools
{
    public static readonly ToolGroup[] Groups =
    [
        new("Tools_Cat_Files",
        [
            new("SpreadsheetMerger", "bi-file-earmark-spreadsheet"),
            new("PdfMerger",         "bi-file-earmark-pdf"),
            new("ImageCompressor",   "bi-file-earmark-image"),
        ]),
        new("Tools_Cat_TextCode",
        [
            new("Base64",       "bi-code-slash"),
            new("TextCase",     "bi-fonts"),
            new("SqlList",      "bi-database"),
            new("CharCounter",  "bi-body-text"),
        ]),
        new("Tools_Cat_DataBrazil",
        [
            new("CpfCnpj",   "bi-person-vcard"),
            new("CepLookup", "bi-geo-alt"),
            new("QrCode",    "bi-qr-code"),
        ]),
        new("Tools_Cat_Utilities",
        [
            new("PasswordGenerator", "bi-shield-lock"),
        ]),
    ];

    public static readonly Tool[] All = [.. Groups.SelectMany(group => group.Tools)];

    public static int Count => All.Length;
}

public sealed record ToolGroup(string CategoryKey, Tool[] Tools);

// A action do controller é também o prefixo das chaves de texto, o que mantém as três
// coisas atadas. Renomear a action sem renomear as chaves quebra no primeiro acesso.
public sealed record Tool(string Action, string Icon)
{
    public string NameKey => $"Tool_{Action}_Name";

    public string DescriptionKey => $"Tool_{Action}_Desc";
}
