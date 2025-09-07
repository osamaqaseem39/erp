# ERP SaaS Solution

A complete Enterprise Resource Planning (ERP) SaaS solution built with ASP.NET Core 8, following SOLID principles and Domain-Driven Design (DDD) patterns.

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8 Web API
- **Database**: SQL Server with Stored Procedures for all CRUD operations
- **API Style**: Hybrid - GraphQL (queries) + REST (mutations)
- **Multi-tenancy**: Full support with TenantId in all tables
- **Authentication**: JWT + Refresh tokens
- **Authorization**: Role-Based Access Control (RBAC)
- **Logging**: Serilog
- **Caching**: Redis
- **Containerization**: Docker-ready

## 📁 Project Structure

```
ERP.SaaS.sln
├── ERP.Api/                     → Presentation layer
│   ├── Controllers/            → REST endpoints per module
│   ├── GraphQL/                → Queries & Mutations per module
│   ├── Middlewares/            → JWT, RBAC, Exception handling
│   └── Program.cs
├── ERP.Application/             → Application services
│   ├── Core/                   → Core master data services
│   ├── HRM/                    → HRM services
│   ├── Finance/                → Accounting, Payroll, Tax
│   ├── CRM/                    → Customer Relationship
│   ├── SCM/                    → Supply Chain & Inventory
│   ├── Sales/                  → Sales & POS
│   ├── Procurement/            → Vendor & Purchase
│   ├── Projects/               → Project management
│   ├── Manufacturing/          → Production & BOM
│   ├── Reports/                → Reporting & BI
│   └── Admin/                  → RBAC, Tenants
├── ERP.Domain/                  → Entities & Value Objects
├── ERP.Infrastructure/          → Persistence & Infrastructure
│   ├── Database/               → SQL + Stored Procedure executor
│   ├── Repositories/           → Repository per module
│   └── Services/               → Logging, Email, Caching
├── ERP.Shared/                  → Shared kernel
│   ├── DTOs/                   → Data transfer objects
│   ├── Exceptions/
│   ├── Helpers/
│   └── Constants/
└── ERP.Tests/                   → Unit & Integration tests
```

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server 2022 or later
- Redis Server
- Docker (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd erp
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection strings**
   - Update `AppConstants.SQL_CONNECTION_STRING` in `ERP.Shared/Constants/AppConstants.cs`
   - Update `AppConstants.REDIS_CONNECTION_STRING` in the same file

4. **Run the application**
   ```bash
   dotnet run --project ERP.Api
   ```

### Using Docker

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

2. **Access the application**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - GraphQL Playground: http://localhost:5000/graphql

## 🔧 Configuration

### Database Setup

1. Create the database:
   ```sql
   CREATE DATABASE ERPSaaS;
   ```

2. Run the stored procedures script:
   ```bash
   sqlcmd -S localhost -d ERPSaaS -i ERP.Infrastructure/Database/StoredProcedures.sql
   ```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: SQL Server connection string
- `Redis__ConnectionString`: Redis connection string

## 📚 API Documentation

### REST Endpoints

- **Authentication**: `/api/auth`
  - `POST /login` - User login
  - `POST /refresh` - Refresh token
  - `POST /logout` - User logout

- **Tenants**: `/api/tenant`
  - `GET /{id}` - Get tenant by ID
  - `GET /` - Get all tenants (paginated)
  - `POST /` - Create tenant
  - `PUT /{id}` - Update tenant
  - `DELETE /{id}` - Delete tenant

### GraphQL

- **Queries**:
  - `getTenant(id: ID!)` - Get tenant by ID
  - `getAllTenants` - Get all tenants
  - `getTenantBySubdomain(subdomain: String!)` - Get tenant by subdomain

- **Mutations**:
  - `addTenant(input: CreateTenantDto!)` - Create tenant
  - `updateTenant(input: UpdateTenantDto!)` - Update tenant
  - `deleteTenant(id: ID!)` - Delete tenant

## 🔐 Authentication & Authorization

### JWT Token Structure

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "User",
  "TenantId": "tenant-id",
  "exp": 1234567890
}
```

### RBAC Roles

- **SuperAdmin**: Full system access
- **Admin**: Tenant-level administration
- **Manager**: Department-level management
- **User**: Basic user access

## 🏢 Multi-tenancy

The system supports multi-tenancy with:
- Tenant isolation at the database level
- Tenant-specific connection strings
- Tenant-aware middleware
- Tenant context in all operations

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ERP.Tests
```

## 📈 Monitoring & Logging

- **Serilog** for structured logging
- **Redis** for caching and session management
- **Health checks** for system monitoring

## 🔄 Development Workflow

1. Create feature branch
2. Implement domain entities
3. Add stored procedures
4. Create repositories
5. Implement application services
6. Add API controllers/GraphQL resolvers
7. Write tests
8. Create pull request

## 📝 Stored Procedure Naming Convention

- **Insert**: `add{Table}`
- **Update**: `update{Table}`
- **Delete**: `delete{Table}`
- **Get by ID**: `get{Table}ById`
- **Get All**: `getAll{Table}`
- **Get All Paged**: `getAll{Table}Paged`
- **Check Exists**: `{table}Exists`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions, please contact the development team or create an issue in the repository.