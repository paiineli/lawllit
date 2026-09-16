# CLAUDE.md — Lawllit

> Instruções deste projeto. Em conflito com qualquer convenção externa, a **consistência
> interna daqui vence**. Sempre responder em **PT-BR**.
>
> Projeto pessoal do Lucas, repositório público. Não é código corporativo e não segue
> processo de chamado, branch ou Pull Request de empresa nenhuma.

## O que é

Três coisas num domínio só.

- **lawllit.com** — serviços, projetos, currículo em PDF e formulário de contato.
- **Ferramentas** — nove utilitários que rodam inteiros no navegador, sem back-end.
- **finance.lawllit.com** — controle financeiro pessoal, com cadastro aberto.

## Arquétipo

API + Site separados, com dois sites. **.NET 10**, `Nullable` e `ImplicitUsings` ligados.

```
Lawllit.Api            Minimal API. Finance/ e Contato/, cada um com Endpoints/ Repositories/ Services/
Lawllit.Model          modelos, contratos e Common/, divididos em Finance/ e Contato/
Lawllit.Repository     cliente HTTP da API, consumido pelos dois sites
Lawllit.Site           lawllit.com
Lawllit.Finance.Site   finance.lawllit.com
Database/              scripts numerados por ordem de execução
```

Fluxo: `View → Controller → Repository (HTTP) → Endpoints → Services → Repositories (Dapper) → PostgreSQL`.

`Repository` existe nos dois lados. No `.Api` é Dapper, no `.Repository` é cliente HTTP.

`Finance` fica em inglês por ser nome próprio, marca e subdomínio. O resto do código dentro
dele é português.

### Entidade e contrato são classes diferentes

`Lawllit.Model/Finance/` guarda a linha da tabela, com o nome das colunas
(`CdCategoria`, `NmCategoria`). `Lawllit.Model/Finance/Contratos/` guarda o que trafega no
HTTP (`Codigo`, `Nome`). As duas nunca viram uma classe só, senão renomear coluna vaza no
JSON. Um arquivo por assunto, sempre `<Assunto>Contratos.cs`.

## Banco

PostgreSQL, conexão `DefaultConnection`. **Sem prefixo de tabela**: `USUARIO`, `CATEGORIA`,
`TRANSACAO`, `CONTATO`.

- Prefixo de coluna: `CD_` PK, `TX_` texto, `NM_` nome, `NR_` número, `VL_` valor,
  `SN_` 'S'/'N', `DT_` data, `HR_` hora.
- **Sem aspas no SQL.** Escrito em `UPPER_SNAKE`, o PostgreSQL dobra para minúsculo e o
  `Dapper.DefaultTypeMap.MatchNamesWithUnderscores` mapeia para `CdTransacao`.
- Bind `@param`, que é a sintaxe do PostgreSQL, e não `:param`, que é a do Oracle.
- **Toda tabela tem `SN_ATIVO`.** Nada é apagado, inativar é `UPDATE ... SET SN_ATIVO = 'N'`
  e todo SELECT filtra `AND SN_ATIVO = 'S'`. A exceção é a exclusão de conta, que apaga de
  verdade, porque a LGPD dá direito à eliminação e conta inativada continua sendo dado guardado.
- **Tipo de transação é texto.** `TX_TIPO` guarda `'RECEITA'`, `'DESPESA'` ou
  `'INVESTIMENTO'`, que são os nomes dos membros do `TipoTransacaoEnum`. Renomear um membro
  exige migração de dados.
- Paginação `OFFSET/FETCH` mais `COUNT` separado, devolvendo `PaginacaoResposta<T>`.
- **Alteração de esquema é arquivo novo numerado.** Nunca editar um script já aplicado.
- **Trocar a senha do banco é `ALTER USER`.** A variável de ambiente do provedor só cria a
  senha quando o volume de dados nasce, editar depois não muda nada no servidor.

## Estilo de código

**Tudo em português**: classe, método, variável local, rota, texto de tela, JavaScript,
atributo `data-*` e `id` de elemento.

| artefato | exemplo | onde |
|---|---|---|
| modelo | `TransacaoModel` | `.Model` |
| contrato | `TransacaoSalvarModel` | `.Model/*/Contratos` |
| repositório de dados | `TransacaoRepository : ITransacaoRepository` | `.Api/Repositories` |
| serviço | `PainelService : IPainelService` | `.Api/Services` |
| endpoints | `TransacaoEndpoints` (`MapTransacoes`) | `.Api/Endpoints` |
| repositório cliente | `TransacaoRepository : ITransacaoRepository` | `.Repository` |
| view model | `TransacaoViewModel` | `.Site/Models` |
| controller | `TransacaoController` | `.Site/Controllers` |

- Método sem sufixo `Async`: `Listar`, `BuscarPorCodigo`, `Criar`, `Alterar`, `Excluir`.
- Rota em kebab-case português: `api/financas/transacoes`.
- Enum com sufixo `Enum`, valores `UPPER`, sem valor numérico explícito, em `.Model/Common/Enums`.
- `sealed class` e **construtor primário** em toda classe com dependência.
- **Sufixo técnico por extenso**: `Model`, `Repository`, `Service`, `Controller`, `Endpoints`,
  `Extensions`, `Filter`. Nunca `MOD` nem `REP`.
- Interface no fim do arquivo, em `#region Interfaces`. **Não usar outra region.**
- **Sem XML-doc em lugar nenhum.** Os dois sites e a API moram no mesmo repositório, não
  existe consumidor externo lendo IntelliSense.

### Comentários

Comentário explica o **porquê** de uma decisão que não é óbvia lendo o código. Nada que
repita o que a linha já diz.

**O que cabe em uma linha fica em uma linha.** Duas só quando são dois fatos distintos que
realmente não cabem. Três nunca.

### Front-end

- O portfólio **não usa framework de CSS**. As classes de layout são escritas à mão.
- O finance usa **Bootstrap**, onde modal, tabela, grid e form control se pagam.
- Por isso **classe CSS acompanha o framework de cada site**: no finance elas ficam em
  inglês, porque convivem com `row`, `col-md-6` e `btn` do Bootstrap, e misturar idioma
  na mesma `<div>` lê pior. No portfólio são todas nossas.
- Sem CSS nem JS inline. Texto de tela fica na view, nunca em arquivo de recurso.
- **Cuidado com renomeação em massa em `.cshtml`.** Nome de variável Razor e nome de classe
  CSS moram no mesmo arquivo, e trocar palavra inteira já quebrou o grid do Bootstrap uma
  vez. Depois de renomear, cruzar toda classe usada na view contra o CSS definido.

## Config e segredos

Segredo **nunca** em `appsettings.json` nem em código.

- **Variável obrigatória não tem valor no `appsettings.json`.** Um valor ali vira fallback
  silencioso e anula a checagem de subida, que existe para derrubar a aplicação quando a
  variável falta. Isso é de propósito, não tratar como bug.
- Valor de desenvolvimento mora no `Properties/launchSettings.json`, que o `dotnet publish`
  não copia para a imagem.
- `appsettings.json` guarda só o que, faltando a variável, não causa dano nenhum.

## Segurança

- **Limitador por IP**, particionado com `RateLimitPartition`. Nunca um balde único.
- O IP do cliente vem do cabeçalho da borda, lido antes do roteamento. **O domínio direto
  do provedor fica desligado nos dois sites**, o acesso é sempre pela borda.
- **CSP sem `unsafe-inline`**, com a lista exata de origens que as páginas carregam.
  Biblioteca de CDN nova exige entrada nova ali.
- **Cookie** com `HttpOnly`, `SameSite=Lax` e `Secure` fora de desenvolvimento. Lax e não
  Strict porque o link de confirmação de e-mail chega de fora.
- **HSTS** de um ano com `IncludeSubDomains`, sem `preload`.
- **JWT validado por inteiro**, incluindo emissor, audiência, validade e assinatura.
- **Sem Dependabot e sem CodeQL.** O ruído de pull request semanal não se paga num projeto
  de um mantenedor só. Versão de pacote sobe à mão.

## Decisões que não devem ser "corrigidas"

1. **Erro de negócio não vira exceção.** O tipo `Resultado` carrega sucesso ou a mensagem
   pronta, e quem vira texto de tela é a camada de cima.
2. **`CancellationToken` em toda chamada async**, do endpoint até o Dapper.
3. **Camada de `Services` na API.** A regra fica fora do endpoint, que só traduz para HTTP.
4. **Interface existe para fronteira de camada ou adaptador de serviço externo.** Interface
   só para "ter interface" sai fora.
5. **Sufixo por extenso**, porque `Controller`, `Endpoints` e `Service` já são por extenso
   e abreviar dois deles destoava.

## Escopo cortado de propósito

Não é bug nem esquecimento.

- **Preferência de moeda.** O app é em real e a preferência só trocava a formatação, sem
  converter valor nenhum.
- **Onboarding de 3 passos.** No lugar dele, o cadastro já cria dez categorias padrão.
- **Bloqueio ao excluir categoria com transação.** Com `SN_ATIVO` a linha continua existindo,
  então o histórico não quebra.

## Entrega

- **Commit direto na `main` é permitido, depois de perguntar.**
- **Push nunca sem o Lucas pedir.** Commit e push são dois pedidos separados.
- **Uma mudança, um commit.** Antes de commitar, procurar todas as pontas: apagar ou
  renomear qualquer coisa exige um `grep` pelo nome dela no repositório inteiro, incluindo
  README, Dockerfile e arquivos de configuração.
