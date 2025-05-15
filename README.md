# 💱 Currency Exchange API

A secure, scalable, and well-architected .NET Web API for retrieving and converting currency exchange rates. Built with **Clean Architecture**, this project demonstrates solid design principles and enterprise-grade patterns including JWT authentication, Serilog logging, Circuit Breaker resilience, and external API integration with pagination support.

---

## ✅ Features

- 🔐 **JWT Authentication**  
  Secure access using JSON Web Tokens. Users can register and log in via the `AuthController`.

- 🔄 **Currency Conversion**  
  Convert amounts between currencies using live data from external providers. Factory Pattern is implemented to switch between providers dynamically.

- 📅 **Latest and Historical Exchange Rates**  
  Retrieve the latest rates or a time series of exchange rates within a specified date range, with **pagination** support for large datasets.

- ❌ **Invalid Currency Validation**  
  Currencies like `TRY`, `PLN`, `THB`, and `MXN` are considered exceptional and excluded from conversions. Custom validators reject requests involving these currencies.

- 🧱 **Clean Architecture**  
  Proper separation of concerns with layers:  
  - **Domain** (Core business logic)  
  - **Application** (CQRS, MediatR handlers, DTOs)  
  - **Infrastructure** (External API implementations like Frankfurter)  
  - **API** (Controllers, filters, middleware)

- 🧪 **CQRS Pattern with MediatR**  
  Queries and commands are handled in a clean and testable way using the MediatR library.

- 🔁 **Factory Pattern**  
  Implemented for currency provider resolution to allow easy extension (e.g., switching from Frankfurter to another provider).

- 📊 **Pagination Support**  
  For historical data retrieval endpoints, with customizable page size and index.

- 🛡 **Resilience with Circuit Breaker**  
  Integrated with **Polly** to gracefully handle external API failures using Circuit Breaker policies.

- 📂 **Serilog Logging**  
  Structured, persistent, and searchable logging for monitoring and debugging.

- 🌐 **Swagger Integration**  
  Full support for testing all endpoints via Swagger UI. JWT bearer token authentication supported through Swagger interface.
  
- 🔁 **Unit Tests**  
  Implemented unit test with using xUnit

---

## 🗂 Project Structure
    /src
    ├── CurrencyExchange.API --> ASP.NET Core Web API (Presentation Layer)
    ├── CurrencyExchange.Application --> Application Layer (CQRS, Validators, DTOs)
    ├── CurrencyExchange.Domain --> Domain Entities and Interfaces
    ├── CurrencyExchange.Infrastructure --> External APIs, Provider Implementations

📌 **Assumptions Made**
- You have SQL Server installed and accessible.

- You need to manually update the appsettings.json file by modifying the accessible SQL user under the DbConnectionOptions section before running the project.

- This project is assumed to run in a development environment initially.

- Redis is not integrated in this version but planned for future releases.

- The caching layer uses in-memory cache (IMemoryCache) for development/testing purposes.

⚙️ **Setup Instructions**

- Clone the repository to your local machine.
- Update the DbConnectionOptions section in appsettings.json to use a valid and accessible SQL Server user.
- Run the project — the database will be created automatically using the Code First approach.
- Upon first run, initial seed data (such as user roles Admin, User) will be inserted into the database.
- Create a new user by sending a request to the api/users/createUser endpoint via Swagger.
- After creating the user, log in through api/auths/login, then copy the returned token. Use Authorize in Swagger with Bearer {token} to access secured endpoints.

## 📦 Dependencies

- `.NET 8+`
- `MediatR`
- `FluentValidation`
- `Polly`
- `Serilog`
- `AspNetCoreRateLimit`
- `Swashbuckle (Swagger)`
- `Microsoft.Extensions.Caching.Memory`

---

## 🔮 Possible Future Enhancements

- Redis Integration: Replace the in-memory cache with a distributed Redis cache to improve performance and scalability across multiple instances.
- Cache Invalidation: Add a cache eviction strategy that ties into authentication or role updates, so sensitive currency data can be revalidated securely.
- Docker Support: A docker-compose.yaml file is planned for deployment simplification. Due to time constraints, it is not included in this version.
- Rate Limiting per User: Enhance the current IP-based rate limiting with user-based rate limiting to protect sensitive endpoints more securely.
- Refresh Tokens: Implement long-term authentication flow with refresh tokens for improved JWT security.
