<p align="center">
  <img src="Lawllit.Web/wwwroot/lawllit.png" alt="lawllit" width="720">
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

a layered solution split into three projects: a web layer (asp.net core mvc, controllers and razor views), an application layer (services and repositories behind interfaces), and a models layer (entities and view models). the web layer depends only on interfaces, wired through dependency injection.

- service layer for business rules, repository pattern for data access
- a small `Result` type carries success or a localized error key, so controllers never branch on exceptions
- mvc areas isolate the finance app from the public site
- fully bilingual (pt-br and en-us) through resource files and a custom culture provider that reads the signed in user claim

**// stack**

`.NET 10` · `ASP.NET Core MVC` · `C#` · `Razor`<br>
`PostgreSQL` · `Dapper` · `Npgsql`<br>
`Cookie Auth` · `Google OAuth` · `BCrypt` · `Brevo` · `AwesomeAPI`<br>
`Bootstrap` · `Chart.js` · `SheetJS` · `pdf-lib` · `JavaScript`<br>
`Docker` · `Railway`

**// under the hood**

- **auth** cookie authentication plus google oauth, with passwords hashed using bcrypt and onboarding state carried in claims
- **email** transactional email for confirmation and password reset, sent through the brevo api over an http client
- **security** rate limiting on auth endpoints, antiforgery tokens, forwarded headers and a set of hardening response headers
- **data** raw sql over dapper on postgres, with parametrized queries and a clean read model
- **tools** spreadsheet and pdf merging that run entirely in the browser, plus base64 and text case helpers
- **live data** exchange rate quotes pulled from the awesomeapi and cached in memory
