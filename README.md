<p align="center">
  <img src="Lawllit.Site/wwwroot/lawllit.png" alt="lawllit" width="720">
</p>

<p align="center">
  personal portfolio and a small product lab.<br>
  finance, tools, and whatever else is worth building.
</p>

<p align="center">
  live at <a href="https://lawllit.com"><b>lawllit.com</b></a>
</p>

---

**// architecture**

four projects, split by responsibility. an http boundary sits between the web layer and the data layer, so the site never opens a database connection and never sees a line of sql.

```
Controller (Site) → Lawllit.Repository (HTTP) → Lawllit.Api (Minimal API) → Dapper → PostgreSQL
```

| project | role |
|---|---|
| `Lawllit.Model` | entities, api contracts, and shared plumbing (result type, pagination, enums, resources) |
| `Lawllit.Api` | minimal api. endpoints, dapper repositories, and business services. the only project that talks to postgres |
| `Lawllit.Repository` | typed http client for the api, consumed only by the site |
| `Lawllit.Site` | asp.net core mvc, controllers and razor views |

- a small `Result` type carries success or a localized error key, so no layer branches on exceptions and the api stays culture-free
- mvc areas isolate the finance app from the public site
- fully bilingual (pt-br and en-us) through resource files and a custom culture provider that reads the signed in user claim

**// stack**

`.NET 10` · `ASP.NET Core MVC` · `C#` · `Razor` · `Minimal API`<br>
`PostgreSQL` · `Dapper` · `Npgsql`<br>
`Cookie Auth` · `JWT` · `Google OAuth` · `BCrypt` · `Brevo` · `AwesomeAPI`<br>
`Bootstrap` · `Chart.js` · `SheetJS` · `pdf-lib` · `JavaScript`<br>
`Scalar` · `Docker` · `Railway` · `GitHub Actions`

**// under the hood**

- **auth** cookie session on the site, jwt between site and api. the api resolves the user from the token claim, never from a route parameter, and a shared key gates every endpoint. passwords hashed with bcrypt
- **email** transactional email for confirmation and password reset, sent through the brevo api. the site hands over a url template, the api fills in the token, so there is no generic send-email endpoint to abuse
- **security** rate limiting on auth endpoints, antiforgery tokens, forwarded headers, a set of hardening response headers, and containers that run as a non-root user
- **data** hand-written sql over dapper, parametrized, paginated in the database rather than in memory, with every async call carrying the request cancellation token
- **tools** spreadsheet and pdf merging that run entirely in the browser, plus base64, qr code, cpf/cnpj and text helpers
- **live data** exchange rate quotes pulled from the awesomeapi and cached in memory

**// deployment**

three services on railway, all fed by this repository. only the site is public.

```
Postgres (volume) ◄── lawllit-api (no public domain) ◄── lawllit (lawllit.com)
```

each service points its config-as-code file at `railway.api.toml` or `railway.site.toml`, which carry the dockerfile path, the healthcheck and the watch paths. the database credential lives only in the api, which is not reachable from the internet.

kestrel binds to `http://[::]:$PORT` on both. railway's private network is ipv6, so a bind to `0.0.0.0` would leave the site unable to reach the api.

**// environment**

nothing sensitive is committed. `appsettings.json` holds only public values. everything below comes from the environment, using the double underscore convention that `IConfiguration` reads natively.

| variable | service | required |
|---|---|:---:|
| `ConnectionStrings__DefaultConnection` | api | yes |
| `Jwt__SecretKey` | api | yes |
| `Api__Key` | api and site, same value | yes |
| `Email__BrevoApiKey` | api | no |
| `Email__From` | api | no |
| `Api__ExchangeRateUrl` | api | no |
| `Api__Url` | site | yes |
| `Authentication__Google__ClientId` | site | no |
| `Authentication__Google__ClientSecret` | site | no |

`Database/schema.sql` creates the three tables from scratch. the `Type` column stores an integer, `0` income, `1` expense, `2` investment, matching `TransactionTypeEnum`.
