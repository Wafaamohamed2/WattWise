##### ⚡ WattWise — (formerly Energy Optimizer)

### Overview
  EnergyOptimizer is a **production-grade** smart energy management system. It doesn't just monitor energy; it **understands** it. By leveraging **Google Gemini AI**, the system detects anomalies, predicts future usage, and provides intelligent recommendations to reduce waste—all broadcasted in real-time via **SignalR WebSockets**.

### Key Features
   ### 1. AI-Driven Intelligence (Google Gemini)
- **Pattern Analysis:** Deep analysis of consumption behaviors across devices.
- **Anomaly Detection:** Real-time flagging of unusual deviations with severity levels.
- **Smart Recommendations:** Actionable, AI-generated tips to save money and energy.
- **Usage Forecasting:** Prediction of future consumption using historical trends.
### 2. Real-Time Ecosystem
- **Live Dashboard:** Instant updates of total consumption and active device counts.
- **Smart Alerts:** Immediate push notifications for spikes, wastage, or offline devices.
- **Zone Control:** Per-zone monitoring using SignalR groups for efficient data broadcasting.
### 3. Advanced Backend Architecture
- **Realistic Simulation:** A sophisticated background service that simulates Egyptian household patterns.
- **Automated Detection:** Continuous background monitoring for consumption spikes and device health.
- **Clean Architecture:** Strict separation of concerns for maximum maintainability.


## Project Structure (Clean Architecture)
        EnergyOptimizer/
       ├── API Layer              # SignalR Hubs, Controllers, Background Services
       ├── Core Layer             # Domain Entities, MediatR CQRS, Interfaces
       ├── Infrastructure Layer   # EF Core DBContext, Repository Implementations
       ├── Service Layer          # AI Logic, Gemini Integration, JWT Logic
       └── Tests Project          # xUnit & Moq Unit Tests

       
### Design Patterns Used
        Pattern                                                    Usage
     CQRS + MediatR                          All business operations separated into Commands/Queries with dedicated Handlers
     Generic Repository + Specification      Flexible, reusable data access with composable query specs
     Clean Architecture                      Strict layer separation; Core has zero external dependencies
     Background Services                     IHostedService for simulator, alert detection, and AI analysis cycles
     Observer (SignalR)                      Real-time event broadcasting to connected clients



## Getting Started
  Prerequisites:
    - .NET 8.0 SDK
    - SQL Server (LocalDB or Express)
    - Google Gemini API Key

## Setup
  **Clone the repository**:
   - bash
    git clone https://github.com/Wafaamohamed2/WattWise.git
  --------------------------------------
  - bash
      - cd EnergyOptimizer.API
      - dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
      -  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SQL_CONNECTION_STRING"
  --------------------------------------
  
  **Apply Migrations**:
   bash
   dotnet ef database update --project ../EnergyOptimizer.Infrastructure --startup-project .
  
  **Run the App**:
  bash
  dotnet run
  
