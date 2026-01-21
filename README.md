# AZFuncCustInvTel - Customer Invoice Telephone Management API

A RESTful API built with Azure Functions (.NET 8 Isolated) for managing customers, invoices, and telephone numbers using Entity Framework Core and SQL Server.

## 📋 Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Features](#features)
- [Development](#development)

## 🎯 Overview

This application provides a complete CRUD API for managing:
- **Customers** - Company or individual customer records with email addresses
- **Invoices** - Financial invoices linked to customers (must start with "INV")
- **Telephone Numbers** - Contact numbers for customers (Mobile, Work, DirectDial)

Key features include:
- Database persistence with SQL Server
- Email uniqueness validation
- Invoice number uniqueness validation
- Related entity management (invoices and phone numbers per customer)
- Calculated customer balance based on invoices
- Comprehensive validation using Data Annotations and FluentValidation

## 🛠️ Technology Stack

- **.NET 8** - Latest LTS version of .NET
- **Azure Functions** - Serverless HTTP-triggered functions (Isolated Worker Process)
- **Entity Framework Core** - ORM for database access
- **SQL Server** - Database
- **FluentValidation** - Advanced validation framework
- **Repository Pattern** - Data access abstraction
- **Application Insights** - Telemetry and monitoring

## 🏗️ Architecture

The application follows a clean architecture approach with clear separation of concerns:

```
┌─────────────────┐
│  HTTP Triggers  │ ← Azure Functions endpoints
└────────┬────────┘
         │
┌────────▼────────┐
│      DTOs       │ ← Data Transfer Objects with validation
└────────┬────────┘
         │
┌────────▼────────┐
│    Mappings     │ ← Extension methods for entity/DTO conversion
└────────┬────────┘
         │
┌────────▼────────┐
│  Repositories   │ ← Data access layer with business logic
└────────┬────────┘
         │
┌────────▼────────┐
│   DbContext     │ ← EF Core database context
└────────┬────────┘
         │
┌────────▼────────┐
│   SQL Server    │ ← Database
└─────────────────┘
```

## 📦 Prerequisites

Before running this application, ensure you have:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (version 8.0 or later)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) (version 4.x)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with Azure Functions extension
- [Azure Storage Emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-emulator) or [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite)

## 🚀 Getting Started

### 1. Clone the Repository

```powershell
git clone <repository-url>
cd AZFuncCustInvTel
```

### 2. Restore Dependencies

```powershell
dotnet restore
```

### 3. Configure Connection String

Update the `local.settings.json` file with your SQL Server credentials:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DefaultConnection": "Server=localhost;Database=efCoreLabs;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Create Database

Run Entity Framework Core migrations to create the database:

```powershell
# Add migration (if not exists)
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 5. Run the Application

```powershell
func start
```

Or press **F5** in Visual Studio.

The API will be available at: `http://localhost:7071`

## ⚙️ Configuration

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DefaultConnection": "Server=localhost;Database=efCoreLabs;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

**Configuration Keys:**
- `AzureWebJobsStorage` - Azure Storage connection (use Azurite for local development)
- `FUNCTIONS_WORKER_RUNTIME` - Must be `dotnet-isolated` for .NET 8
- `DefaultConnection` - SQL Server connection string
- `LocalHttpPort` - Local development port (default: 7071)
- `CORS` - CORS configuration for local testing

## 🗄️ Database Setup

### Entity Relationship Diagram

```
┌─────────────────┐
│    Customer     │
│─────────────────│
│ Id (PK)         │
│ Name            │
│ Email (Unique)  │
│ Balance (Calc)  │
└────────┬────────┘
         │
         │ 1:N
         │
    ┌────┴────────────────┐
    │                     │
┌───▼──────────┐   ┌──────▼───────────┐
│   Invoice    │   │ TelephoneNumber  │
│──────────────│   │──────────────────│
│ Id (PK)      │   │ Id (PK)          │
│ InvoiceNum   │   │ CustomerId (FK)  │
│ CustomerId   │   │ Type             │
│ InvoiceDate  │   │ Number           │
│ Amount       │   └──────────────────┘
└──────────────┘
```

### Database Constraints

**Customer:**
- Email must be unique
- Email max length: 200 characters
- Name max length: 200 characters

**Invoice:**
- InvoiceNumber must be unique
- InvoiceNumber must start with "INV"
- InvoiceNumber max length: 50 characters
- Amount must be >= 0
- Amount stored as decimal(18,2)

**TelephoneNumber:**
- Type must be one of: Mobile, Work, DirectDial
- Number max length: 50 characters
- Type max length: 20 characters

## 📡 API Endpoints

### Customer Endpoints

#### Create Customer
```http
POST http://localhost:7071/api/customers
Content-Type: application/json

{
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "invoices": [
    {
      "invoiceNumber": "INV-001",
      "invoiceDate": "2024-01-15",
      "amount": 1500.00
    }
  ],
  "phoneNumbers": [
    {
      "type": "Work",
      "number": "+1-555-0100"
    }
  ]
}
```

**Response: 201 Created**
```json
{
  "id": 1,
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "balance": 1500.00,
  "invoices": [...],
  "phoneNumbers": [...]
}
```

#### Get All Customers
```http
GET http://localhost:7071/api/customers
```

**Response: 200 OK**
```json
[
  {
    "id": 1,
    "name": "Acme Corporation",
    "email": "contact@acme.com",
    "balance": 1500.00,
    "invoices": [...],
    "phoneNumbers": [...]
  }
]
```

#### Get Customer by ID
```http
GET http://localhost:7071/api/customers/{id}
```

**Response: 200 OK** (includes related invoices and phone numbers)
```json
{
  "id": 1,
  "name": "Acme Corporation",
  "email": "contact@acme.com",
  "balance": 1500.00,
  "invoices": [
    {
      "id": 1,
      "invoiceNumber": "INV-001",
      "customerId": 1,
      "invoiceDate": "2024-01-15",
      "amount": 1500.00
    }
  ],
  "phoneNumbers": [
    {
      "id": 1,
      "customerId": 1,
      "type": "Work",
      "number": "+1-555-0100"
    }
  ]
}
```

#### Update Customer
```http
PUT http://localhost:7071/api/customers/{id}
Content-Type: application/json

{
  "name": "Acme Corp Updated",
  "email": "new-email@acme.com"
}
```

**Response: 200 OK**

#### Delete Customer
```http
DELETE http://localhost:7071/api/customers/{id}
```

**Response: 200 OK**
```json
{
  "message": "Customer with id 1 deleted successfully."
}
```

### Validation Rules

**CreateCustomerDto:**
- `name`: Required, max 200 characters
- `email`: Required, valid email format, max 200 characters, must be unique
- `invoices[].invoiceNumber`: Required, must start with "INV", max 50 characters
- `invoices[].invoiceDate`: Required
- `invoices[].amount`: Required, must be >= 0
- `phoneNumbers[].type`: Must be "Mobile", "Work", or "DirectDial"
- `phoneNumbers[].number`: Max 50 characters

**UpdateCustomerDto:**
- `name`: Required, max 200 characters
- `email`: Required, valid email format, max 200 characters, must be unique

### Error Responses

**400 Bad Request** - Validation errors
```json
{
  "errors": {
    "Email": ["Email is required.", "Invalid email address format."],
    "Name": ["Name cannot exceed 200 characters."]
  }
}
```

**404 Not Found** - Resource not found
```json
{
  "error": "Customer with id 999 not found."
}
```

**409 Conflict** - Duplicate email
```json
{
  "error": "A customer with email 'contact@acme.com' already exists."
}
```

## 📁 Project Structure

```
AZFuncCustInvTel/
├── DTOs/
│   ├── CustomerDto.cs          # Customer data transfer objects
│   ├── InvoiceDto.cs           # Invoice data transfer objects
│   └── TelephoneNumberDto.cs   # Phone number data transfer objects
├── Functions/
│   └── CustomerFunctions.cs    # Customer HTTP endpoints
├── Mappings/
│   └── MappingExtensions.cs    # Entity/DTO mapping extensions
├── Models/
│   ├── AppDbContext.cs         # EF Core database context
│   ├── Customer.cs             # Customer entity
│   ├── Invoice.cs              # Invoice entity
│   └── TelephoneNumber.cs      # Phone number entity
├── Repositories/
│   ├── IRepositories.cs        # Repository interfaces
│   ├── CustomerRepository.cs   # Customer data access
│   ├── InvoiceRepository.cs    # Invoice data access
│   └── TelephoneNumberRepository.cs
├── Validators/
│   ├── CreateCustomerValidator.cs
│   └── UpdateCustomerValidator.cs
├── Program.cs                  # Application entry point & DI configuration
├── local.settings.json         # Local configuration (not in source control)
└── AZFuncCustInvTel.csproj    # Project file
```

## ✨ Features

### Implemented Features

✅ **Customer Management**
- Create customers with invoices and phone numbers
- Update customer information
- Retrieve single customer with related data
- List all customers
- Delete customers
- Email uniqueness validation

✅ **Data Validation**
- Data Annotations for basic validation
- FluentValidation for complex rules
- Business rule validation (email uniqueness, invoice number format)

✅ **Database**
- Entity Framework Core with SQL Server
- Repository pattern for data access
- Include related entities (eager loading)
- Unique constraints and indexes
- Calculated properties (Customer.Balance)

✅ **Best Practices**
- Dependency injection
- Async/await pattern
- Proper HTTP status codes
- Logging with ILogger
- CORS configuration for local development

### Planned Features

⏳ Invoice management endpoints  
⏳ Telephone number management endpoints  
⏳ Pagination for list endpoints  
⏳ Search and filtering  
⏳ Authentication/Authorization  
⏳ Unit tests  
⏳ Integration tests  
⏳ Swagger/OpenAPI documentation  

## 🔧 Development

### Building the Project

```powershell
dotnet build
```

### Running Tests

```powershell
dotnet test
```

### Creating a Migration

```powershell
dotnet ef migrations add MigrationName
```

### Applying Migrations

```powershell
dotnet ef database update
```

### Reverting a Migration

```powershell
dotnet ef database update PreviousMigrationName
```

### Removing Last Migration

```powershell
dotnet ef migrations remove
```

## 📝 Testing with PowerShell

### Create a Customer

```powershell
$body = @'
{
  "name": "Test Company",
  "email": "test@company.com",
  "invoices": [
    {
      "invoiceNumber": "INV-12345",
      "invoiceDate": "2024-01-15",
      "amount": 1000.00
    }
  ],
  "phoneNumbers": [
    {
      "type": "Mobile",
      "number": "+1-555-1234"
    }
  ]
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/customers" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

### Get All Customers

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/customers" -Method Get
```

### Get Customer by ID

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/customers/1" -Method Get
```

### Update Customer

```powershell
$body = @'
{
  "name": "Updated Company Name",
  "email": "updated@company.com"
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/customers/1" `
  -Method Put `
  -ContentType "application/json" `
  -Body $body
```

### Delete Customer

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/customers/1" -Method Delete
```

## 🐛 Troubleshooting

### Database Connection Issues

**Error:** "Cannot connect to SQL Server"

**Solution:** 
- Verify SQL Server is running
- Check connection string in `local.settings.json`
- Ensure SQL Server allows TCP/IP connections
- Verify firewall settings

### Migration Issues

**Error:** "No migrations found"

**Solution:**
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Azure Functions Runtime

**Error:** "The listener for function 'CreateCustomer' was unable to start"

**Solution:**
- Check port 7071 is not in use
- Verify Azure Functions Core Tools is installed
- Restart the application


