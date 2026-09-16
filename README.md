<p align="center">
  <img src="Lawllit.Site/wwwroot/lawllit.png" alt="lawllit" width="720">
</p>

<p align="center">
  personal portfolio and a small product lab.<br>
  finance, tools, and whatever else is worth building.
</p>

<p align="center">
  live at <a href="https://lawllit.com"><b>lawllit.com</b></a>
  and <a href="https://finance.lawllit.com"><b>finance.lawllit.com</b></a>
</p>

---

**// architecture**

five projects, split by responsibility. an http boundary sits between the web layer and the data layer, so neither site opens a database connection or sees a line of sql.

```
View → Controller (Site) → Lawllit.Repository (HTTP) → Lawllit.Api → Services → Dapper → PostgreSQL
```

| project | role |
|---|---|
| `Lawllit.Model` | entities, api contracts, and shared plumbing (result type, pagination, enums) |
| `Lawllit.Api` | minimal api. endpoints, dapper repositories, and business services. the only project that talks to postgres |
| `Lawllit.Repository` | typed http client for the api, consumed by both sites |
| `Lawllit.Site` | `lawllit.com`. services, projects, browser tools and the contact form |
| `Lawllit.Finance.Site` | `finance.lawllit.com`. the finance app, cookie session of its own |

the api holds no screen and the sites hold no sql, which is what makes the split worth the extra hop: the finance app was an mvc area of the portfolio until it outgrew the arrangement, and separating them meant each one deploys, scales and fails on its own.

entities carry the database shape (`CdTransacao`, `TxDescricao`) while contracts carry the api shape (`Codigo`, `Descricao`). the two never collapse into one class, so a column rename does not leak into a json payload.

**// the finance app**

a personal finance tracker with open registration. it is public on purpose: sign-up, email confirmation and password recovery are the parts that show how the whole thing is wired.

- **dashboard** month or year, with a doughnut of spending per category and a stacked trend of the last months. every card is computed server-side and handed to the view as one model
- **transactions** filtered by type, month and search, paginated in the database, with csv export that opens correctly in a pt-BR excel
- **recurring** a transaction marked recurring is copied into the next month on demand, and the screen shows how many are still pending for the month being viewed
- **spending vs history** each category is compared against its own average in previous months rather than a budget the user has to maintain. no history means no comparison, not a 100% drop
- **categories** created per user, ten of them seeded on sign-up so a new account works on first login
- **exchange rates** live quotes, cached in memory, failing with a message instead of an empty screen
- **profile** three themes including high contrast, three font sizes, and real account deletion

**// the portfolio**

a landing page aimed at clients rather than recruiters, plus nine browser tools that run with no back-end at all: spreadsheet merging, pdf merging, image compression, base64, sql `in` lists, cpf/cnpj, postal code lookup, qr codes and a password generator. the server only ships the page.

**// engineering decisions**

- **business errors are not exceptions.** a `Resultado` type carries success or the message itself, and the layer above turns it into screen text. nothing branches on a `try/catch` to handle the predictable
- **services sit between endpoints and repositories.** the endpoint translates http and nothing else, so a rule can be read without reading routing
- **every async call carries the request cancellation token**, from the endpoint down to dapper. a user closing the tab stops the query
- **transaction type is stored as text**, holding `RECEITA`, `DESPESA` or `INVESTIMENTO`, which are the member names of the enum. a row reads without a lookup table, at the cost of a migration if a member is ever renamed
- **nothing is deleted.** rows carry `SN_ATIVO` and are deactivated. the single exception is account deletion, which erases user, categories and transactions for real, because a deactivated account is still stored personal data
- **everything is portuguese**, from class names to routes to screen text, including the javascript. translation is a later problem
- **comments explain the why, never the what.** a comment that restates the line it sits on was removed

**// security**

- passwords hashed with bcrypt, and a malformed hash fails the login instead of the request
- cookie session on the site, jwt between site and api, validated whole, signature included. the api resolves the user from the token claim and never from a route parameter
- a shared key gates every api endpoint, which has no public domain and is reached over the private network
- rate limiting partitioned per client ip rather than one global bucket, so one abuser cannot close the door for everyone
- content security policy with no `unsafe-inline`, hsts, hardened session and antiforgery cookies
- the public contact form is verified server-side with cloudflare turnstile, plus a honeypot field and a send limit
- containers run as a non-root user, and a missing required secret stops the app from starting rather than failing later

**// performance**

- the portfolio ships **no css framework**. the twenty-odd layout classes it borrowed were rewritten by hand, which took 232 KB off the critical path. the finance app keeps bootstrap, where modals, tables and form controls actually earn it
- fonts are self-hosted as one variable file, with `unicode-range` so the extended latin set is only fetched by pages that need it
- the icon font is subset to the twelve icons actually used, down from around two thousand
- brotli and gzip on, measured rather than guessed: the fast level shipped a larger file than the cdn, the maximum cost half a second of cpu per request
- fingerprinted assets cached for a year, everything else for an hour

**// data**

hand-written sql over dapper, parametrized, paginated in the database rather than in memory. four tables: `USUARIO`, `CATEGORIA`, `TRANSACAO` and `CONTATO`.

identifiers are unquoted `UPPER_SNAKE`, so postgres folds them to lowercase and dapper's `MatchNamesWithUnderscores` lands `CD_TRANSACAO` on `CdTransacao`. schema changes are new numbered files under `Database/`, never an edit to one already applied.

**// infrastructure**

four services on railway, all fed by this repository: postgres, the api, and the two sites. each service builds from its own dockerfile and carries its deploy config as code, so a change to one site does not redeploy the other two. the api is not reachable from the internet.

kestrel binds to `http://[::]:$PORT`. railway's private network is ipv6, so a bind to `0.0.0.0` would leave the sites unable to reach the api.

nothing sensitive is committed. every secret comes from the environment.

**// stack**

`.NET 10` · `ASP.NET Core MVC` · `C#` · `Razor` · `Minimal API`<br>
`PostgreSQL` · `Dapper` · `Npgsql`<br>
`Cookie Auth` · `JWT` · `BCrypt` · `Resend` · `Cloudflare Turnstile`<br>
`Bootstrap` · `Chart.js` · `SheetJS` · `pdf-lib` · `JavaScript`<br>
`Scalar` · `Docker` · `Railway`

---

<p align="center">
  <sub>built by <a href="https://github.com/paiineli">paiineli</a>.</sub>
</p>
