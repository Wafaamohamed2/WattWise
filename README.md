##### ⚡ WattWise — (formerly Energy Optimizer)

![Build & Test CI](https://github.com/Wafaamohamed2/WattWise/actions/workflows/ci.yml/badge.svg)

### Overview
  WattWise is a full-stack smart energy monitoring platform built on .NET 8 with Clean Architecture and CQRS. It ingests live device readings, streams them to clients over SignalR WebSockets, and uses Google Gemini AI to detect anomalies, forecast consumption, and generate actionable savings recommendations — all in real time.

The backend simulates realistic household energy patterns (based on Egyptian household usage behavior) so the whole system can be explored end-to-end without physical IoT hardware.

### Key Features
   ### 1. AI-Driven Intelligence (Google Gemini)
- **Pattern Analysis:** deep analysis of consumption behaviors across devices and zones.
- **Anomaly Detection:** real-time flagging of unusual deviations with severity levels.
- **Smart Recommendations:** actionable, AI-generated tips to cut cost and waste.
- **Usage Forecasting:** prediction of future consumption using historical trends.
### 2. Real-Time Ecosystem
- **Live Dashboard:** instant updates of total consumption and active device via SignalR.
- **Smart Alerts:** push notifications for spikes, wastage, or offline devices.
- **Zone Control:** SignalR groups scoped per zone for efficient data delivery.
### 3. Backend Architecture
- **Clean Architecture:** strict separation of concerns, zero external dependencies in the domain layer.
- **CQRS + MediatR:** every operation is a Command/Query handled independentlypatterns.
- **Generic Repository + Specification Pattern:** composable, reusable data access.
- **JWT Authentication:** access + refresh token flow, email verification, password reset.
- **Background Services:** hosted services for simulation, alert detection, AI analysis cycles, and token cleanup.



## LayeredLayered Structure
       WattWise/
       ├── EnergyOptimizer.API              # Controllers, SignalR Hubs, Middleware, Background Services
       ├── EnergyOptimizer.Core              # Entities, CQRS (Commands/Queries/Handlers), Interfaces, DTOs
       ├── EnergyOptimizer.Infrastructure    # EF Core DbContext, Generic Repository, Identity, Current User
       ├── EnergyOptimizer.Service           # AI/Gemini logic, JWT, Email, Alerts, Pattern Detection
       ├── EnergyOptimizer.Data              # DbContext, Migrations, ApplicationUser (Identity)
       ├── EnergyOptimizer.Tests             # xUnit + Moq unit & integration tests
       └── WattWise-Frontend                 # Static HTML/JS client (dashboard, auth, charts, AI views)

### Request flow
 
<img width="9872" height="5671" alt="Untitled-2026-08-15-2037 excalidraw" src="https://github.com/user-attachments/assets/2d356c35-58e0-462e-9ee5-20de0d4a70d7" />


### Design Patterns Used
        Pattern                                                    Usage
     CQRS + MediatR                          All business operations separated into Commands/Queries with its own Handler
     Generic Repository + Specification      Flexible, reusable data access with composable query specs
     Clean Architecture                      Strict layer separation; Core has zero external dependencies
     Background Services(IHostedService)     Simulator, alert detection, and AI analysis cycles run independently of requests
     Observer (SignalR)                      Real-time broadcasting of readings, alerts, and AI insights to connected clients
     Pipeline Behaviors                      Cross-cutting concerns (validation, caching, cache invalidation) applied to every                                                 MediatR request


## Getting Started
  Prerequisites:
    - .NET 8.0 SDK
    - SQL Server (LocalDB or Express)
    - Google Gemini API Key
    - (Optional) Redis — for distributed caching
    - (Optional) RabbitMQ — falls back to in-memory transport if not available

## Setup
  **1. Clone the repository**:
   - bash
        git clone https://github.com/Wafaamohamed2/WattWise.git

  **2. Configure secrets**
   - bash
      - cd EnergyOptimizer.API
      - dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
      - dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SQL_CONNECTION_STRING"
  
  **3. Apply Migrations**:
   bash
   dotnet ef database update --project ../EnergyOptimizer.Infrastructure --startup-project .
  
  **4. Run the App**:
  bash
  dotnet run
  
  The API will start seeding demo data and simulating live readings automatically. Swagger UI is available at https://localhost:  {port}/swagger.

  **5. Run the frontend**
     Serve WattWise-Frontend/ with any static file server (e.g. VS Code Live Server on port 5500) — this matches the default AllowedOrigins / FrontendUrl CORS config. Open index.html after registering a user via login.html / register.html.


### 5. Run the frontend

Serve WattWise-Frontend/ with any static file server (e.g. VS Code Live Server on port 5500) — this matches the default AllowedOrigins / FrontendUrl CORS config. Open index.html after registering a user via login.html / register.html.
  
## 5. Run the frontend

Serve WattWise-Frontend/ with any static file server (e.g. VS Code Live Server on port 5500) — this matches the default AllowedOrigins / FrontendUrl CORS config. Open index.html after registering a user via login.html / register.html.


## Tech Stack
  **Backend:** .NET 8 · ASP.NET Core Web API · Entity Framework Core · MediatR · AutoMapper · FluentValidation · SignalR · Serilog · Swashbuckle · MassTransit (RabbitMQ) Auth: ASP.NET Core Identity · JWT Bearer + Refresh Tokens AI: Google Gemini API Caching: In-Memory / Redis (StackExchangeRedis) Database: SQL Server
  **Testing:** xUnit · Moq CI/CD: GitHub Actions Frontend: HTML5 · Vanilla JavaScript · SignalR JS Client
