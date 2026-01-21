# AZFuncCustInvTel - Student Exercise

## 🎓 Learning Objectives

This Azure Functions application demonstrates RESTful API development with .NET 8, Entity Framework Core, and SQL Server. Your task is to **complete the Invoice and TelephoneNumber endpoints** following the established patterns.

## 📚 What's Already Implemented

✅ **Customer Endpoints (COMPLETE)** - Study these as reference implementations:
- `CustomerFunctions.cs` - Uses Data Annotations for validation
- `CustomerFunctionsWithFluentValidation.cs` - Uses FluentValidation for validation

Both implementations include:
- Create Customer (POST)
- Get All Customers (GET)
- Get Customer by ID (GET)
- Update Customer (PUT)
- Delete Customer (DELETE)

✅ **Repository Pattern** - Already implemented for all entities:
- `ICustomerRepository` / `CustomerRepository`
- `IInvoiceRepository` / `InvoiceRepository`
- `ITelephoneNumberRepository` / `TelephoneNumberRepository`

✅ **Data Models** - Already defined:
- `Customer`, `Invoice`, `TelephoneNumber` entities
- DTOs with validation attributes
- EF Core DbContext configuration

✅ **Mapping Extensions** - Already implemented in `MappingExtensions.cs`:
- `ToDto()` - Convert entity to DTO
- `ToEntity()` - Convert DTO to entity
- `UpdateEntity()` - Update entity from DTO

## 🎯 Your Assignment

### ⚠️ TODO: Implement Invoice Endpoints

Create a new file `InvoiceFunctions.cs` with the following endpoints:

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/invoices` | Create a new invoice |
| GET | `/api/invoices` | Get all invoices |
| GET | `/api/invoices/{id}` | Get invoice by ID |
| PUT | `/api/invoices/{id}` | Update an invoice |
| DELETE | `/api/invoices/{id}` | Delete an invoice |
| GET | `/api/invoices/customer/{customerId}` | Get invoices for a specific customer |

### ⚠️ TODO: Implement TelephoneNumber Endpoints

Create a new file `TelephoneNumberFunctions.cs` with the following endpoints:

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/telephone-numbers` | Create a new telephone number |
| GET | `/api/telephone-numbers` | Get all telephone numbers |
| GET | `/api/telephone-numbers/{id}` | Get telephone number by ID |
| PUT | `/api/telephone-numbers/{id}` | Update a telephone number |
| DELETE | `/api/telephone-numbers/{id}` | Delete a telephone number |
| GET | `/api/telephone-numbers/customer/{customerId}` | Get telephone numbers for a specific customer |

## 📋 Implementation Guidelines

### Step 1: Create the Functions Class

Follow the pattern from `CustomerFunctions.cs`:

```csharp
using AZFuncCustInvTel.DTOs;
using AZFuncCustInvTel.Mappings;
using AZFuncCustInvTel.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AZFuncCustInvTel.Functions
{
    public class InvoiceFunctions
    {
        private readonly ILogger<InvoiceFunctions> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;

        public InvoiceFunctions(
            ILogger<InvoiceFunctions> logger, 
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository)
        {
            _logger = logger;
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
        }

        // TODO: Implement your endpoints here
    }
}
```

### Step 2: Implement Create Endpoint

Example pattern for creating an invoice:

```csharp
[Function("CreateInvoice")]
public async Task<IActionResult> CreateInvoice(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "invoices")] HttpRequestData req)
{
    _logger.LogInformation("CreateInvoice called.");

    // 1. Deserialize request body
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    CreateInvoiceDto? dto;
    try
    {
        dto = JsonSerializer.Deserialize<CreateInvoiceDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (JsonException ex)
    {
        _logger.LogWarning(ex, "Invalid JSON.");
        return new BadRequestObjectResult(new { error = "Invalid JSON payload." });
    }

    if (dto == null)
    {
        return new BadRequestObjectResult(new { error = "Request body is empty." });
    }

    // 2. Validate the DTO
    var validationErrors = ValidateModel(dto);
    if (validationErrors.Any())
    {
        return new BadRequestObjectResult(new { errors = validationErrors });
    }

    // 3. Check if customer exists
    if (!await _customerRepository.ExistsAsync(dto.CustomerId))
    {
        return new BadRequestObjectResult(new { error = $"Customer with id {dto.CustomerId} not found." });
    }

    // 4. Check if invoice number already exists
    if (await _invoiceRepository.InvoiceNumberExistsAsync(dto.InvoiceNumber))
    {
        return new BadRequestObjectResult(new { error = $"Invoice with number '{dto.InvoiceNumber}' already exists." });
    }

    // 5. Map DTO to entity and save
    var invoice = dto.ToEntity();
    var createdInvoice = await _invoiceRepository.CreateAsync(invoice);
    var invoiceDto = createdInvoice.ToDto();

    return new CreatedResult($"/api/invoices/{invoiceDto.Id}", invoiceDto);
}

// Helper method for validation
private static IDictionary<string, string[]> ValidateModel(object model)
{
    var context = new ValidationContext(model, serviceProvider: null, items: null);
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(model, context, results, validateAllProperties: true);

    var errors = results
        .SelectMany(r =>
            r.MemberNames.Any()
                ? r.MemberNames.Select(m => new { Member = m, Message = r.ErrorMessage ?? string.Empty })
                : new[] { new { Member = string.Empty, Message = r.ErrorMessage ?? string.Empty } })
        .GroupBy(x => x.Member)
        .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

    return errors;
}
```

### Step 3: Implement Other Endpoints

Follow the same pattern for:
- `GetAllInvoices` - Get all invoices
- `GetInvoice` - Get invoice by ID
- `UpdateInvoice` - Update invoice
- `DeleteInvoice` - Delete invoice
- `GetInvoicesByCustomer` - Get invoices by customer

**See the CustomerFunctions.cs file for complete examples of each endpoint type.**

## 🧪 Testing Your Endpoints

### Test Create Invoice

```powershell
$body = @'
{
  "invoiceNumber": "INV-12345",
  "customerId": 1,
  "invoiceDate": "2024-01-15",
  "amount": 1500.00
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/invoices" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

### Test Get All Invoices

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/invoices" -Method Get
```

### Test Get Invoice by ID

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/1" -Method Get
```

### Test Get Invoices by Customer

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/customer/1" -Method Get
```

### Test Update Invoice

```powershell
$body = @'
{
  "invoiceNumber": "INV-12345-UPDATED",
  "invoiceDate": "2024-01-20",
  "amount": 2000.00
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/1" `
  -Method Put `
  -ContentType "application/json" `
  -Body $body
```

### Test Delete Invoice

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/1" -Method Delete
```

### Test Create Telephone Number

```powershell
$body = @'
{
  "customerId": 1,
  "type": "Mobile",
  "number": "+1-555-0100"
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

### Test Get All Telephone Numbers

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers" -Method Get
```

### Test Get Telephone Number by ID

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers/1" -Method Get
```

### Test Get Telephone Numbers by Customer

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers/customer/1" -Method Get
```

### Test Update Telephone Number

```powershell
$body = @'
{
  "type": "Work",
  "number": "+1-555-0200"
}
'@

Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers/1" `
  -Method Put `
  -ContentType "application/json" `
  -Body $body
```

### Test Delete Telephone Number

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers/1" -Method Delete
```

## ✅ Validation Requirements

### Invoice Validation Rules
- ✅ `InvoiceNumber`: Required, must start with "INV", max 50 characters, must be unique
- ✅ `CustomerId`: Required, must be greater than 0, customer must exist
- ✅ `InvoiceDate`: Required
- ✅ `Amount`: Required, must be >= 0

### TelephoneNumber Validation Rules
- ✅ `CustomerId`: Required, must be greater than 0, customer must exist
- ✅ `Type`: Required, must be one of: "Mobile", "Work", "DirectDial"
- ✅ `Number`: Required, max 50 characters

## 🔍 Key Concepts to Apply

### 1. Dependency Injection
```csharp
// Inject repositories in constructor
public InvoiceFunctions(
    ILogger<InvoiceFunctions> logger, 
    IInvoiceRepository invoiceRepository,
    ICustomerRepository customerRepository)
{
    _logger = logger;
    _invoiceRepository = invoiceRepository;
    _customerRepository = customerRepository;
}
```

### 2. Repository Pattern
```csharp
// Use repository methods for data access
var invoice = await _invoiceRepository.GetByIdAsync(id);
var createdInvoice = await _invoiceRepository.CreateAsync(invoice);
var updatedInvoice = await _invoiceRepository.UpdateAsync(invoice);
var deleted = await _invoiceRepository.DeleteAsync(id);
```

### 3. DTO Mapping
```csharp
// Convert between DTOs and entities using mapping extensions
var entity = dto.ToEntity();           // DTO -> Entity
var dto = entity.ToDto();              // Entity -> DTO
dto.UpdateEntity(existingEntity);      // Update existing entity from DTO
```

### 4. Validation
```csharp
// Validate DTOs using Data Annotations
var validationErrors = ValidateModel(dto);
if (validationErrors.Any())
{
    return new BadRequestObjectResult(new { errors = validationErrors });
}
```

### 5. Business Rules
```csharp
// Check business rules before persisting
if (!await _customerRepository.ExistsAsync(dto.CustomerId))
{
    return new BadRequestObjectResult(new { error = "Customer not found." });
}

if (await _invoiceRepository.InvoiceNumberExistsAsync(dto.InvoiceNumber))
{
    return new BadRequestObjectResult(new { error = "Invoice number already exists." });
}
```

### 6. HTTP Status Codes
```csharp
// Return appropriate status codes
return new CreatedResult($"/api/invoices/{id}", dto);     // 201 Created
return new OkObjectResult(dto);                           // 200 OK
return new NotFoundObjectResult(new { error = "..." });   // 404 Not Found
return new BadRequestObjectResult(new { error = "..." }); // 400 Bad Request
```

## 📝 Checklist

Use this checklist to track your progress:

### Invoice Endpoints
- [ ] Create `InvoiceFunctions.cs` file
- [ ] Inject `IInvoiceRepository` and `ICustomerRepository`
- [ ] Implement `CreateInvoice` (POST)
  - [ ] Validate DTO
  - [ ] Check customer exists
  - [ ] Check invoice number uniqueness
  - [ ] Create and return invoice
- [ ] Implement `GetAllInvoices` (GET)
- [ ] Implement `GetInvoice` (GET by ID)
- [ ] Implement `UpdateInvoice` (PUT)
  - [ ] Validate DTO
  - [ ] Check invoice exists
  - [ ] Check invoice number uniqueness (excluding current)
  - [ ] Update and return invoice
- [ ] Implement `DeleteInvoice` (DELETE)
- [ ] Implement `GetInvoicesByCustomer` (GET by customer)
- [ ] Test all endpoints with PowerShell

### TelephoneNumber Endpoints
- [ ] Create `TelephoneNumberFunctions.cs` file
- [ ] Inject `ITelephoneNumberRepository` and `ICustomerRepository`
- [ ] Implement `CreateTelephoneNumber` (POST)
  - [ ] Validate DTO
  - [ ] Check customer exists
  - [ ] Create and return telephone number
- [ ] Implement `GetAllTelephoneNumbers` (GET)
- [ ] Implement `GetTelephoneNumber` (GET by ID)
- [ ] Implement `UpdateTelephoneNumber` (PUT)
  - [ ] Validate DTO
  - [ ] Check telephone number exists
  - [ ] Update and return telephone number
- [ ] Implement `DeleteTelephoneNumber` (DELETE)
- [ ] Implement `GetTelephoneNumbersByCustomer` (GET by customer)
- [ ] Test all endpoints with PowerShell

## 🎓 Learning Resources

### Reference Implementations
- Study `CustomerFunctions.cs` for the complete pattern
- Review `CustomerRepository.cs` for repository examples
- Check `MappingExtensions.cs` for mapping patterns
- See `CustomerFunctionsWithFluentValidation.cs` for FluentValidation examples

### Documentation
- [Azure Functions .NET 8 Isolated](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

## 🚀 Running the Application

1. **Start the Azure Functions runtime:**
   ```powershell
   func start
   ```

2. **Or run from Visual Studio:**
   - Press `F5` to start debugging
   - The API will be available at `http://localhost:7071`

3. **Test your endpoints:**
   - Use the PowerShell examples above
   - Or use a tool like Postman or Thunder Client

## 💡 Tips for Success

1. **Start with Invoice endpoints** - They're similar to Customer endpoints
2. **Test as you go** - Don't wait until everything is implemented
3. **Copy and adapt** - Use CustomerFunctions.cs as a template
4. **Read error messages** - They usually tell you exactly what's wrong
5. **Check the repositories** - They already have all the methods you need
6. **Use the mapping extensions** - Don't manually map properties
7. **Follow the patterns** - Consistency makes your code maintainable
8. **Ask for help** - If you're stuck, let me know!

## 🏆 Potential Learning Extensions

Once you've completed the basic endpoints, try these advanced features:

1. **FluentValidation Version** - Create `InvoiceFunctionsWithFluentValidation.cs`
2. **FluentValidation Version** - Create `TelephoneFunctionsWithFluentValidation.cs`
