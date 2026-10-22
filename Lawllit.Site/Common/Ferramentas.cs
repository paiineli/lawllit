namespace Lawllit.Site.Common;

// Num lugar só porque a home e a página de ferramentas precisam da mesma lista.
public static class Ferramentas
{
    public static readonly GrupoFerramenta[] Grupos =
    [
        new("arquivos", "Arquivos",
        [
            new("JuntarPlanilhas", "planilha", "Juntar planilhas", "Junte várias planilhas do Excel numa só, sem perder nenhuma linha."),
            new("JuntarPdf", "camadas", "Juntar PDF", "Junte vários PDFs num arquivo só, com todas as páginas na ordem."),
            new("ComprimirImagem", "imagem", "Comprimir imagem", "Deixe fotos mais leves para caber no e-mail ou no formulário."),
            new("ComprimirPdf", "comprimir", "Comprimir PDF", "Diminua o tamanho de PDFs escaneados ou cheios de fotos."),
        ]),
        new("texto", "Texto",
        [
            new("Base64", "codigo", "Base64", "Converta texto para Base64 e de volta, linha por linha."),
            new("ListaSql", "banco-dados", "Lista para SQL", "Transforme uma lista colada num filtro pronto para a consulta."),
        ]),
        new("brasil", "Dados do Brasil",
        [
            new("CpfCnpj", "identidade", "CPF e CNPJ", "Confira se um CPF ou CNPJ é válido, formate ou gere um para teste."),
            new("ConsultaCep", "mapa", "Consulta de CEP", "Digite o CEP e veja rua, bairro, cidade e DDD."),
            new("QrCode", "qr", "QR Code", "Crie um QR Code de qualquer link ou texto e baixe a imagem."),
        ]),
        new("utilidades", "Utilidades",
        [
            new("GerarSenha", "cadeado", "Gerador de senha", "Crie senhas fortes e aleatórias, do tamanho que precisar."),
        ]),
    ];

    public static readonly Ferramenta[] Todas = [.. Grupos.SelectMany(grupo => grupo.Ferramentas)];

    public static int Quantidade => Todas.Length;

    // O cabeçalho de cada página de ferramenta se descreve a partir daqui, em vez de
    // repetir nome e descrição que já estão escritos na lista.
    public static Ferramenta Por(string acao)
        => Todas.First(ferramenta => ferramenta.Acao == acao);
}

// A âncora vira o id do grupo no índice, para o atalho do topo descer direto até ele.
public sealed record GrupoFerramenta(string Ancora, string Categoria, Ferramenta[] Ferramentas);

// A action do controller anda junto do nome, o que impede de renomear uma sem a outra.
public sealed record Ferramenta(string Acao, string Icone, string Nome, string Descricao);
