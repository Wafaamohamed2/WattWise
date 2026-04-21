##### ⚡ EnergyOptimizer — AI-Powered Energy Management System

### Overview
  EnergyOptimizer is a production-grade smart energy management system that monitors electricity consumption across devices and zones in real time, detects anomalies using AI, and generates actionable recommendations to reduce energy waste — all powered by Google Gemini AI and delivered live via SignalR WebSockets.
  The system simulates a smart home/building with multiple devices (ACs, water heaters, TVs, lights, etc.), generates realistic energy readings every minute, and continuously runs AI-driven analysis in the background.

### Features
   ## 1- Real-Time Monitoring 
        - Live energy readings broadcast to all clients via SignalR WebSockets
        - Real-time dashboard updates: total consumption, active devices, top consumers
        - Device status change notifications (on/off toggle)
        - Per-zone live readings through SignalR groups
   ## 2- AI-Powered Analysis (Google Gemini)
        - Pattern Analysis — Identifies consumption patterns across devices and time periods
        - Anomaly Detection — Flags unusual consumption deviations with severity levels (Low / Medium / High / Critical)
        - Smart Recommendations — Generates actionable energy-saving suggestions
        - Consumption Prediction — Forecasts future energy usage based on historical data
        - Automated background analysis cycle (configurable interval)
   ## 3- Intelligent Alert System
        - Automatic high-consumption detection 
        - Sudden spike detection (200%+ increase in one reading cycle)
        - Wastage alerts (devices running at unusual hours)
        - Device offline detection
        - Real-time alert push via SignalR
   ## 4- Analytics & Charts
        - Hourly, daily, and zone-based consumption breakdown
        - Device type distribution 
        - Top consumers ranking
        - Consumption trend over time
        - Exportable charts (PNG download)
   ## 5- Security
        - JWT authentication with HttpOnly cookie support + Bearer header fallback
        - Rate limiting on auth endpoints (10 req/min)
        - Role-based authorization (Admin for seeding)
        - Centralized exception middleware with environment-aware error responses

### Architecture 

EnergyOptimizer/
├── API Layer                  # Controllers, Hubs, Middleware, Background Services
│   ├── Controllers/           # REST endpoints (AI, Alerts, Dashboard, Devices, Readings)
│   ├── Hubs/                  # EnergyHub (SignalR)
│   ├── Services/              # Background services (Simulator, AlertDetection, AIAnalysis)
│   └── Middleware/            # Exception handling
│
├── Core Layer                 # Domain entities, interfaces, DTOs, CQRS
│   ├── Entities/              # Device, Zone, Building, EnergyReading, Alert, AI analysis models
│   ├── Features/              # CQRS: Commands, Queries, Handlers (MediatR)
│   ├── Specifications/        # Specification pattern for queries
│   ├── Interfaces/            # Repository, service contracts
│   └── Exceptions/            # Domain exceptions
│
├── Infrastructure Layer       # Data access, repository implementation
│   ├── Data/                  # EF Core DbContext
│   └── Repositories/          # GenericRepository + SpecificationEvaluator
│
└── Service Layer              # Business logic
    └── Services/              # AIAnalysisService, GeminiService, PatternDetectionService,
                               # DataCleanupService, JwtTokenService


### Design Patterns Used
      ## Pattern                                         ## Usage
     CQRS + MediatR                          All business operations separated into Commands/Queries with dedicated Handlers
     Generic Repository + Specification      Flexible, reusable data access with composable query specs
     Clean Architecture                      Strict layer separation; Core has zero external dependencies
     Background Services                     IHostedService for simulator, alert detection, and AI analysis cycles
     Observer (SignalR)                      Real-time event broadcasting to connected clients


### Key Background Services
  1. EnergyReadingSimulatorService
        Generates realistic energy readings every minute based on device type, time of day, and day of week.
        Simulates Egyptian household patterns (ACs peak in the afternoon, water heaters peak morning/evening, etc.).
        Broadcasts to all SignalR clients immediately after saving.
  2. AlertDetectionService
        Runs every 30 seconds. Checks for high consumption (150%+ above 5-reading baseline), sudden spikes (200%+ jump), wastage (devices on during off-hours), and device offline conditions.
        Creates alerts in DB and pushes them via SignalR.
  3. AIAnalysisBackgroundService
        Runs the full global AI analysis cycle at a configurable interval (default: every 24 hours). Includes pattern analysis, anomaly detection, and recommendation generation.


### Data Model Overview
         Building → Zone → Device → EnergyReading
                                  → Alert
         EnergyAnalysis → AnalysisInsight
                       → EnergyRecommendation
                       → DetectedAnomaly → Device
         ConsumptionPrediction
         UsagePattern
