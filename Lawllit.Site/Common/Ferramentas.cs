namespace Lawllit.Site.Common;

// Num lugar só porque a home e a página de ferramentas precisam da mesma lista.
public static class Ferramentas
{
    public static readonly GrupoFerramenta[] Grupos =
    [
        new("// arquivos",
        [
            new("JuntarPlanilhas", "bi-file-earmark-spreadsheet", "unificador de planilhas", "una múltiplos arquivos .xlsx e .xls sem perder dados."),
            new("JuntarPdf", "bi-file-earmark-pdf", "unificador de pdf", "junte múltiplos arquivos .pdf em um só, preservando todas as páginas."),
            new("ComprimirImagem", "bi-file-earmark-image", "comprimir imagem", "reduza o tamanho de imagens .jpg e .png pra caber em e-mails e uploads."),
            new("ComprimirPdf", "bi-file-earmark-pdf", "comprimir pdf", "reduza o tamanho de arquivos .pdf escaneados ou com muitas imagens."),
        ]),
        new("// texto",
        [
            new("Base64", "bi-code-slash", "base64", "codifique e decodifique texto no formato base64."),
            new("ListaSql", "bi-database", "lista para sql", "transforme uma lista de valores em cláusula in do sql, com ou sem aspas."),
        ]),
        new("// dados & brasil",
        [
            new("CpfCnpj", "bi-person-vcard", "cpf / cnpj", "valide e gere números de cpf e cnpj, com ou sem máscara."),
            new("ConsultaCep", "bi-geo-alt", "consulta de cep", "consulte endereços a partir do cep usando a base dos correios."),
            new("QrCode", "bi-qr-code", "qr code", "gere um qr code a partir de qualquer texto ou link e baixe em png."),
        ]),
        new("// utilidades",
        [
            new("GerarSenha", "bi-shield-lock", "gerador de senha", "crie senhas fortes e aleatórias, geradas no seu navegador."),
        ]),
    ];

    public static readonly Ferramenta[] Todas = [.. Grupos.SelectMany(grupo => grupo.Ferramentas)];

    public static int Quantidade => Todas.Length;

    // O cabeçalho de cada página de ferramenta se descreve a partir daqui, em vez de
    // repetir nome e descrição que já estão escritos na lista.
    public static Ferramenta Por(string acao)
        => Todas.First(ferramenta => ferramenta.Acao == acao);
}

public sealed record GrupoFerramenta(string Categoria, Ferramenta[] Ferramentas);

// A action do controller anda junto do nome, o que impede de renomear uma sem a outra.
public sealed record Ferramenta(string Acao, string Icone, string Nome, string Descricao);
