# FinanceBillingPlatform
A full-stack .NET 8 finance platform for managing invoices and payments, built as a Great Learning capstone project.

## Business Objective
Reduce revenue leakage through automated invoice tracking, payment processing, and overdue detection.

## Technology Stack
| Layer | Technology |
|---|---|
| Backend | C# 8.0, ASP.NET Core 8, SOLID Principles |
| Database | MS SQL Server, EF Core 8, ADO.NET |
| Web UI | ASP.NET Core MVC, Razor Pages, Bootstrap 5 |
| API | RESTful Web API, JWT Authentication, Swagger |
| Testing | xUnit, Moq, FluentAssertions (28 tests) |
| DevOps | Docker, GitHub Actions CI/CD, Azure App Services |

## Project Structure
FinanceBillingPlatform/
├── FinanceBilling.Core/          # Domain entities, interfaces, services
├── FinanceBilling.Infrastructure/ # EF Core, repositories, ADO.NET
├── FinanceBilling.Web/            # ASP.NET Core MVC application
├── FinanceBilling.API/            # RESTful Web API with JWT
├── FinanceBilling.Tests/          # xUnit unit tests (28 tests)
└── DevOps/                        # Dockerfile, CI/CD YAML, Copilot logs
## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full)
- Visual Studio 2022

### Database Setup
1. Open SSMS and connect to `(localdb)\MSSQLLocalDB`
2. Run scripts in order from `Database/` folder

### Running the Application

**Web Application:**
```bash
cd FinanceBilling.Web
dotnet run
```
Navigate to `https://localhost:{port}`

**API:**
```bash
cd FinanceBilling.API
dotnet run
```
Navigate to `https://localhost:{port}/swagger`

### Running Tests
```bash
dotnet test FinanceBilling.Tests/FinanceBilling.Tests.csproj
```

## API Authentication
1. POST `/api/Auth/login` with credentials below
2. Copy the token from response
3. Click Authorize in Swagger and enter `Bearer {token}`

| Username | Password | Role |
|---|---|---|
| admin | Admin@123 | Admin |
| manager | Manager@123 | Manager |
| viewer | Viewer@123 | Viewer |

## API Endpoints
| Method | Endpoint | Description | Role |
|---|---|---|---|
| POST | /api/Auth/login | Get JWT token | Public |
| GET | /api/Invoices | List all invoices | All |
| POST | /api/Invoices | Create invoice | Admin, Manager |
| GET | /api/Invoices/{id} | Get invoice details | All |
| POST | /api/Invoices/{id}/items | Add item | Admin, Manager |
| POST | /api/Invoices/{id}/send | Send invoice | Admin, Manager |
| POST | /api/Invoices/{id}/payments | Process payment | Admin, Manager |
| GET | /api/Invoices/overdue | Get overdue invoices | All |

## Core Business Rules
- Invoices start as Draft and follow: Draft → Sent → PartiallyPaid → Paid
- Items can only be added to Draft invoices
- Payments cannot exceed the balance due
- Paid invoices cannot be cancelled
- Due date must always be in the future when creating
- SQL triggers auto-update TotalAmount and PaidAmount
- All changes to invoices are recorded in AuditLogs

## SOLID Principles Applied
- **S** — Each class has one responsibility (InvoiceService, InvoiceRepository separate)
- **O** — BaseEntity extended without modification
- **L** — Invoice and Payment both safely extend BaseEntity
- **I** — IInvoiceRepository extends IRepository only with invoice-specific methods
- **D** — Controllers depend on IInvoiceService not InvoiceService directly

## Testing
28 unit tests covering:
- Invoice creation, validation, status transitions
- Payment creation, completion, failure
- Service layer with mocked repositories
- All custom exception scenarios

## DevOps
- **Docker**: Multi-stage Dockerfile for minimal image size
- **CI/CD**: GitHub Actions pipeline builds, tests, and deploys on push to main
- **Azure**: App Services for API and Web, Azure SQL for database
- **Copilot**: 30 prompts logged in `DevOps/copilot-prompts.txt`
