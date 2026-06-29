# Atlier API

Atlier is an AI-assisted fashion marketplace and print-on-demand platform. This repository contains the ASP.NET Core API that powers its design studio, storefront, community, order fulfillment, administration, and real-time notifications.

## Related repository

The Angular frontend is available at [ReemAbdelkader/Frontend-Graduation-Project](https://github.com/ReemAbdelkader/Frontend-Graduation-Project).

## Highlights

- JWT authentication, refresh tokens, email confirmation, and Google sign-in
- User, printer, and administrator roles
- Product catalog, categories, images, carts, checkout, and order management
- Saved garment designs, graphic assets, snapshots, and reusable templates
- AI chat, image generation, onboarding suggestions, and admin reports
- Community feed with likes, saves, comments, creators, and moderation reports
- Printer assignment and production-status workflows
- Rewards and user dashboard analytics
- SignalR notifications
- Swagger/OpenAPI documentation

## Tech stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 9 / ASP.NET Core Web API |
| Database | SQL Server |
| Data access | Entity Framework Core 9 |
| Authentication | ASP.NET Core Identity, JWT Bearer, Google OAuth |
| Architecture | Layered architecture with CQRS and MediatR |
| Mapping and validation | Mapster and FluentValidation |
| Real-time updates | SignalR |
| Payments | Stripe |
| Email | MailKit / SMTP |
| API documentation | Swagger / OpenAPI |
| Tests | NUnit, Moq, and FluentAssertions |

## Solution structure

| Project | Responsibility |
| --- | --- |
| `ITIGraduationProject.Domain` | Entities, enums, constants, and domain rules |
| `ITIGraduationProject.Application` | CQRS features, DTOs, validation, interfaces, and mappings |
| `ITIGraduationProject.Service` | Identity, email, files, AI, reports, notifications, and pricing services |
| `ITIGraduationProject.Infrastructure` | EF Core, SQL Server, Identity, repositories, payments, SignalR, and seeders |
| `ITIGraduationProject.Api` | Controllers, middleware, Swagger, static files, and application startup |
| `ITIGraduationProject.Test` | Unit tests for application handlers and services |

## Prerequisites

- .NET 9 SDK
- SQL Server 2019 or newer (SQL Server Express is also suitable)
- EF Core CLI tools
- Optional integrations as needed: SMTP account, Google OAuth, Stripe, and the configured AI services
- The Atlier Angular frontend for the complete application experience

Install the EF Core command-line tool if it is not already available:

```bash
dotnet tool install --global dotnet-ef --version 9.*
```

## Configuration

The API reads configuration from `ITIGraduationProject.Api/appsettings.json`, environment-specific settings, environment variables, and .NET user secrets. The main sections are:

| Section | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnectionString` | SQL Server connection |
| `JwtSettings` | JWT secret, issuer, audience, and token lifetime |
| `ClientSettings:ClientBaseUrl` | Frontend URL used in email links |
| `AdminSeed`, `PrinterSeed`, `CommunitySeed` | Optional initial accounts |
| `EmailSettings` | SMTP sender configuration |
| `Authentication:Google` | Google OAuth credentials |
| `AILayer` and `DifyAI` | AI workflow endpoints and credentials |
| `ReportGenerator` | Admin analytics/report workflow |
| `Stripe` | Payment API credentials |

For local development, keep secrets out of tracked settings and store them with the API project's configured user-secrets ID:

```bash
dotnet user-secrets --project ITIGraduationProject.Api set "ConnectionStrings:DefaultConnectionString" "Server=.;Database=ITIGraduationProjectDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets --project ITIGraduationProject.Api set "JwtSettings:SecretKey" "replace-with-a-long-random-development-secret"
dotnet user-secrets --project ITIGraduationProject.Api set "EmailSettings:AppPassword" "your-smtp-app-password"
dotnet user-secrets --project ITIGraduationProject.Api set "Authentication:Google:ClientSecret" "your-google-client-secret"
dotnet user-secrets --project ITIGraduationProject.Api set "Stripe:SecretKey" "your-stripe-test-secret"
```

Add credentials only for integrations you intend to use. Environment variables are recommended in deployed environments; use double underscores for nested keys, for example `JwtSettings__SecretKey`.

> Never commit real passwords, API keys, OAuth secrets, or payment credentials. If a secret has previously been committed, remove it from the repository and rotate it with its provider.

## Run locally

From the solution directory:

1. Restore packages:

   ```bash
   dotnet restore
   ```

2. Apply the database migrations:

   ```bash
   dotnet ef database update --project ITIGraduationProject.Infrastructure --startup-project ITIGraduationProject.Api
   ```

3. Start the API:

   ```bash
   dotnet run --project ITIGraduationProject.Api
   ```

4. Open Swagger at [http://localhost:5135/swagger](http://localhost:5135/swagger).

The API listens on `http://localhost:5135` in the included launch profile. The Angular development client is allowed from `http://localhost:4200`.

At startup, roles and configured seed accounts are created when missing. In the Development environment, deterministic demo users, products, templates, designs, orders, and community activity are also seeded. Apply migrations before the first run because startup seeding expects the database schema to exist.

## Database migrations

Create a migration after changing the persistence model:

```bash
dotnet ef migrations add <MigrationName> --project ITIGraduationProject.Infrastructure --startup-project ITIGraduationProject.Api
```

Apply pending migrations:

```bash
dotnet ef database update --project ITIGraduationProject.Infrastructure --startup-project ITIGraduationProject.Api
```

## API areas

Swagger is the source of truth for request and response schemas. Major endpoint groups include:

- `/api/Identity` — registration, login, tokens, password flows, OAuth, and onboarding
- `/api/Products`, `/api/Categories`, `/api/Templates` — catalog and templates
- `/api/DesignStudio` — products, designs, assets, snapshots, and AI generation
- `/api/Cart`, `/api/Orders` — cart and ordering workflows
- `/api/community` — feed, interactions, comments, reports, and creators
- `/api/Profiles`, `/api/user-dashboard`, `/api/Rewards` — customer profile and dashboard
- `/api/printer`, `/api/PrinterProfiles` — printer operations
- `/api/admin/*` — users, orders, analytics, AI reports, and moderation
- `/api/Notifications` and `/hubs/notifications` — notification history and live SignalR events

Protected REST endpoints expect a bearer token:

```http
Authorization: Bearer <access-token>
```

Swagger's **Authorize** action can add the header while testing endpoints.

## Static files and uploads

Uploaded content is served from the API's `wwwroot` directory. The server accepts request bodies up to 50 MB and exposes paths used by the frontend for product images, templates, graphic assets, and design snapshots. Treat uploaded files as runtime data and store them in persistent external storage for production deployments.

## Build and test

Build the full solution:

```bash
dotnet build ITIGraduationProject.sln
```

Run the test suite:

```bash
dotnet test ITIGraduationProject.sln
```

Create a release build:

```bash
dotnet publish ITIGraduationProject.Api -c Release -o ./publish
```

## Deployment notes

- Set `ASPNETCORE_ENVIRONMENT` appropriately and supply secrets through the host environment or a secrets manager.
- Use a production SQL Server connection and apply migrations during deployment.
- Update the allowed CORS origin and `ClientSettings:ClientBaseUrl` to the deployed frontend URL.
- Configure persistent storage for uploads.
- Serve the API behind HTTPS.
- Verify SMTP, OAuth callback URLs, Stripe web settings, and AI service availability before enabling those features.
