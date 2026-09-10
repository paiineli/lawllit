namespace Lawllit.Site.Common;

// Estrutura do currículo. Aqui fica só o que não muda de idioma, o contato, o nome das
// empresas e a contagem de itens de cada seção. Todo texto traduzido vive no resx e é
// buscado pela chave que cada item carrega.
//
// Antes disso o currículo eram dois HTML soltos no wwwroot, um por idioma, então trocar
// de cargo custava três edições e a versão em inglês ia ficando para trás.
public static class Curriculum
{
    public const string Name = "Lucas Paineli Gimenes";
    public const string Email = "painelilucas@gmail.com";
    public const string LinkedIn = "linkedin.com/in/paiineli";
    public const string GitHub = "github.com/paiineli";

    public const int AboutParagraphs = 4;
    public const int Educations = 3;
    public const int Certifications = 4;
    public const int Languages = 2;

    public static readonly CurriculumJob[] Jobs =
    [
        new("Cv_Job1", "Unimed Sorocaba", Bullets: 7, HasStack: true),
        new("Cv_Job2", "Unimed Sorocaba", Bullets: 5, HasStack: true),
        new("Cv_Job3", "Unimed Sorocaba", Bullets: 3, HasStack: false),
        new("Cv_Job4", "Instituto de Diagnósticos de Sorocaba", Bullets: 0, HasStack: false),
    ];
}

// A linha de stack está no resx, e não aqui, porque a ordem de "APIs REST" inverte em
// inglês. O que fica no registro é só quantos tópicos o cargo tem e se ele tem stack.
public sealed record CurriculumJob(string Key, string Organization, int Bullets, bool HasStack)
{
    public string TitleKey => $"{Key}_Title";

    public string PeriodKey => $"{Key}_Period";

    public string IntroKey => $"{Key}_Intro";

    public string StackKey => $"{Key}_Stack";

    public string BulletKey(int number) => $"{Key}_Bullet{number}";
}
