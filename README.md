# lawllit

Source repository for [lawllit.com](https://lawllit.com) and its products.

## Products

**lawllit.finance** - Personal finance management web application. Tracks income, expenses, recurring transactions, and real-time exchange rates across multiple currencies.

## Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core MVC |
| Database | PostgreSQL via Entity Framework Core |
| Auth | Cookie-based + Google OAuth |
| Email | Brevo (transactional) |
| Deploy | Docker on Railway |

## Architecture

The solution is split into two projects:

- **Lawllit.Data** — EF Core models, migrations, and DbContext
- **Lawllit.Web** — MVC application structured with Areas: public company site and product-specific areas (Finance)

Localization support for pt-BR and en-US. Theme switching across light, dark, and high contrast modes.

## License

See [LICENSE](LICENSE).
