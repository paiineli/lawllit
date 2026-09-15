namespace Lawllit.Site.Common;

// Consumido pela home e pela página de currículo, para o texto não ficar repetido nas duas.
public static class Curriculo
{
    public const string Nome = "Lucas Paineli Gimenes";
    public const string Email = "contato@lawllit.com";
    public const string LinkedIn = "linkedin.com/in/paiineli";
    public const string GitHub = "github.com/paiineli";

    public const string Titulo = "Desenvolvedor Fullstack .NET | C#, ASP.NET Core, Oracle | APIs REST · Clean Architecture · IA no fluxo de trabalho";

    public const string Competencias = "C#, ASP.NET Core, .NET Framework, Oracle Database, SQL, PL/SQL, APIs REST, Clean Architecture, SOLID, Design Patterns, Clean Code, Entity Framework, Azure DevOps, Git, JavaScript, HTML5, CSS3, Bootstrap, Razor, SQL Server, Azure SQL, Scrum, Code Review, Linux, Windows Server, TFS";

    // O primeiro parágrafo abre a home, então a seção "sobre" de lá começa no segundo.
    public static readonly string[] Sobre =
    [
        "Desenvolvedor Fullstack .NET com 6 anos em tecnologia, atuando com C#, ASP.NET Core, .NET Framework e Oracle Database em sistemas web de saúde que precisam funcionar o tempo todo.",
        "No dia a dia trabalho em toda a stack: modelagem e otimização de dados em Oracle com SQL e PL/SQL, construção e consumo de APIs REST, e desenvolvimento de interfaces com HTML5, CSS3, JavaScript, Bootstrap e Razor. Aplico Clean Architecture, SOLID e Design Patterns, participo de code reviews e faço versionamento com Git e Azure DevOps.",
        "Uso IA de verdade no trabalho: GitHub Copilot, Claude e Cursor fazem parte do meu fluxo diário para escrever, revisar e depurar código mais rápido. Não como substituição de raciocínio, mas como ferramenta que acelera entrega sem abrir mão da qualidade.",
        "Tenho Especialização em Arquitetura de Software pela Facens (nota 9,6) e MBA Executivo em Gestão Empresarial pela FGV (nota 9,22). Busco uma posição pleno ou software engineer em .NET, preferencialmente em empresas com volume de dados maior e sistemas com requisitos críticos de disponibilidade.",
    ];

    public static readonly Cargo[] Cargos =
    [
        new("Analista Programador Web Jr",
            "Unimed Sorocaba",
            "Out 2024 – Atual",
            "Desenvolvo e mantenho sistemas web de saúde em C# e .NET, atuando diariamente em toda a stack, da camada de dados até o front-end.",
            [
                "Desenvolvo back-end em C# com ASP.NET Core e .NET Framework, aplicando Clean Architecture, SOLID e Design Patterns.",
                "Construo e consumo APIs REST integradas a fluxos clínicos e operacionais da empresa.",
                "Escrevo e otimizo queries, stored procedures, triggers e funções PL/SQL em Oracle Database.",
                "Desenvolvo interfaces front-end com HTML5, CSS3, JavaScript, Bootstrap e Razor.",
                "Participo de code reviews com o time.",
                "Uso GitHub Copilot, Claude e Cursor no fluxo diário para acelerar e revisar código.",
                "Faço versionamento com Git, TFS e Azure DevOps.",
            ],
            "C# · ASP.NET Core · .NET Framework · Oracle Database · SQL/PL/SQL · APIs REST · HTML5 · CSS3 · JavaScript · Bootstrap · Razor · Azure DevOps · Git · TFS"),
        new("Analista de Sistemas PACS",
            "Unimed Sorocaba",
            "Fev 2023 – Set 2024",
            "Administração de sistemas hospitalares PACS, RIS e HIS, com foco em banco de dados Oracle e suporte técnico avançado em produção.",
            [
                "Escrevi e otimizei scripts SQL e PL/SQL para automação de processos e extração de dados.",
                "Trabalhei em performance de queries em bases com alto volume de dados.",
                "Atuei como ponto de escalonamento N3 em falhas complexas de integração e performance.",
                "Ajudei na administração de servidores Linux e Windows, incluindo rotinas de recuperação.",
                "Acompanhei o time e fornecedores em upgrades e aplicação de patches.",
            ],
            "Oracle Database · SQL/PL/SQL · Linux · Windows Server"),
        new("Assistente de Suporte Técnico PACS",
            "Unimed Sorocaba",
            "Jun 2021 – Jan 2023",
            "Suporte técnico N2 e N3 para mais de 300 profissionais de saúde em ambiente de alta disponibilidade.",
            [
                "Diagnostiquei e resolvi incidentes de visualização, armazenamento e transmissão de imagem médica.",
                "Configurei e mantive estações de trabalho e modalidades de imagem, incluindo CT, MRI e X-Ray.",
                "Ajudei na administração de servidores Linux e Windows.",
            ],
            null),
        new("Estagiário de TI",
            "Instituto de Diagnósticos de Sorocaba",
            "Out 2020 – Jun 2021",
            "Suporte técnico N1 e N2, instalação e manutenção de sistemas internos, administração básica de Windows Server e Active Directory.",
            [],
            null),
    ];

    public static readonly Item[] Formacao =
    [
        new("MBA Executivo, Gestão Empresarial", "Fundação Getulio Vargas (FGV) · Jan 2025 – Mai 2026 · Nota 9,22/10"),
        new("Pós-Graduação, Arquitetura de Software", "Centro Universitário Facens · Fev 2023 – Mai 2025 · Nota 9,6/10"),
        new("Tecnólogo, Análise e Desenvolvimento de Sistemas", "Universidade Paulista (UNIP) · Fev 2021 – Dez 2022"),
    ];

    public static readonly Item[] Certificacoes =
    [
        new("Imersão Digital – Trilha Especialista em IA", "Santander Open Academy · Jul 2026"),
        new("English Semi Intensive Program (Advanced 2)", "OHLA Schools, Miami · Mai 2026"),
        new("Foundational C# with Microsoft", "Microsoft / freeCodeCamp · Mar 2026"),
        new("Introdução à Ciência de Dados", "Fundação Getulio Vargas (FGV) · Ago 2022"),
    ];

    public static readonly Item[] Idiomas =
    [
        new("Português", "Nativo"),
        new("Inglês", "Avançado"),
    ];

    // Destaque e demais ficam separados porque o destaque muda o peso do chip na home.
    public static readonly GrupoCompetencia[] GruposCompetencia =
    [
        new("linguagem e front",
            ["C#", "ASP.NET Core"],
            [".NET Framework", "Razor", "JavaScript", "HTML5", "CSS3", "Bootstrap"]),
        new("dados",
            ["Oracle Database", "SQL", "PL/SQL"],
            ["SQL Server", "Azure SQL", "Entity Framework"]),
        new("arquitetura e prática",
            ["APIs REST"],
            ["Clean Architecture", "SOLID", "Design Patterns", "Clean Code", "Code Review", "Scrum"]),
        new("ferramenta e infra",
            ["Azure DevOps", "Git"],
            ["TFS", "Linux", "Windows Server"]),
    ];
}

public sealed record Cargo(string Titulo, string Empresa, string Periodo, string Introducao, string[] Topicos, string? Stack);

public sealed record Item(string Titulo, string Detalhe);

public sealed record GrupoCompetencia(string Rotulo, string[] Destaque, string[] Demais);
