using AZFuncCustInvTel.DTOs;
using AZFuncCustInvTel.Mappings;
using AZFuncCustInvTel.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AZFuncCustInvTel.Functions
{
    /// <summary>
    /// Alternative implementation of CustomerFunctions using FluentValidation.
    /// This demonstrates a different validation approach compared to DataAnnotations.
    /// 
    /// COMPARISON FOR STUDENTS:
    /// - DataAnnotations: Declarative, attribute-based, simpler for basic scenarios
    /// - FluentValidation: Code-based, more flexible, better for complex rules
    /// 
    /// This version includes full CRUD operations with database persistence.
    /// </summary>
    public class CustomerFunctionsWithFluentValidation
    {
        private readonly ILogger<CustomerFunctionsWithFluentValidation> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IValidator<CreateCustomerDto> _createCustomerValidator;
        private readonly IValidator<UpdateCustomerDto> _updateCustomerValidator;

        // Constructor with dependency injection for validators and repository
        public CustomerFunctionsWithFluentValidation(
            ILogger<CustomerFunctionsWithFluentValidation> logger,
            ICustomerRepository customerRepository,
            IValidator<CreateCustomerDto> createCustomerValidator,
            IValidator<UpdateCustomerDto> updateCustomerValidator)
        {
            _logger = logger;
            _customerRepository = customerRepository;
            _createCustomerValidator = createCustomerValidator;
            _updateCustomerValidator = updateCustomerValidator;
        }

        [Function("CreateCustomerFluentValidation")]
        public async Task<IActionResult> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers-fluent")] HttpRequestData req)
        {
            _logger.LogInformation("CreateCustomer (FluentValidation) called.");

            // Deserialize request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            CreateCustomerDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<CreateCustomerDto>(body, new JsonSerializerOptions
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
                return new BadRequestObjectResult(new { error = "Request body is empty or could not be deserialized." });
            }

            // FluentValidation approach
            var validationResult = await _createCustomerValidator.ValidateAsync(dto);
            
            if (!validationResult.IsValid)
            {
                // Convert FluentValidation errors to a dictionary format
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return new BadRequestObjectResult(new { errors });
            }

            // Check for duplicate email
            if (await _customerRepository.EmailExistsAsync(dto.Email))
            {
                return new BadRequestObjectResult(new { error = $"A customer with email '{dto.Email}' already exists." });
            }

            // Map DTO to entity and persist to database
            var customer = dto.ToEntity();
            var createdCustomer = await _customerRepository.CreateAsync(customer);
            var customerDto = createdCustomer.ToDto();

            return new CreatedResult($"/api/customers-fluent/{customerDto.Id}", customerDto);
        }

        [Function("UpdateCustomerFluentValidation")]
        public async Task<IActionResult> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers-fluent/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("UpdateCustomer (FluentValidation) called for id {Id}.", id);

            // Deserialize request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            UpdateCustomerDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<UpdateCustomerDto>(body, new JsonSerializerOptions
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
                return new BadRequestObjectResult(new { error = "Request body is empty or could not be deserialized." });
            }

            // FluentValidation approach
            var validationResult = await _updateCustomerValidator.ValidateAsync(dto);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return new BadRequestObjectResult(new { errors });
            }

            // Load existing customer
            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            // Check for duplicate email (excluding current customer)
            if (await _customerRepository.EmailExistsAsync(dto.Email, id))
            {
                return new BadRequestObjectResult(new { error = $"A customer with email '{dto.Email}' already exists." });
            }

            // Apply updates and save changes
            dto.UpdateEntity(existingCustomer);
            var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
            var customerDto = updatedCustomer.ToDto();

            return new OkObjectResult(customerDto);
        }

        [Function("GetCustomerFluentValidation")]
        public async Task<IActionResult> GetCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers-fluent/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("GetCustomer (FluentValidation) called for id {Id}.", id);

            var customer = await _customerRepository.GetByIdAsync(id, includeRelated: true);
            if (customer == null)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            var customerDto = customer.ToDto();
            return new OkObjectResult(customerDto);
        }

        [Function("GetAllCustomersFluentValidation")]
        public async Task<IActionResult> GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers-fluent")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllCustomers (FluentValidation) called.");

            var customers = await _customerRepository.GetAllAsync(includeRelated: true);
            var customerDtos = customers.Select(c => c.ToDto()).ToList();

            return new OkObjectResult(customerDtos);
        }

        [Function("DeleteCustomerFluentValidation")]
        public async Task<IActionResult> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers-fluent/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("DeleteCustomer (FluentValidation) called for id {Id}.", id);

            var deleted = await _customerRepository.DeleteAsync(id);
            if (!deleted)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            return new OkObjectResult(new { message = $"Customer with id {id} deleted successfully." });
        }

        // TODO FOR STUDENTS: Implement Invoice and Telephone Number endpoints
        // 
        // EXERCISE: Create the following Azure Functions endpoints following the pattern above:
        // 
        // Invoice Endpoints:
        // - POST /api/invoices-fluent - Create invoice
        // - GET /api/invoices-fluent - Get all invoices
        // - GET /api/invoices-fluent/{id} - Get invoice by ID
        // - PUT /api/invoices-fluent/{id} - Update invoice
        // - DELETE /api/invoices-fluent/{id} - Delete invoice
        // - GET /api/invoices-fluent/customer/{customerId} - Get invoices by customer
        //
        // Telephone Number Endpoints:
        // - POST /api/telephone-numbers-fluent - Create telephone number
        // - GET /api/telephone-numbers-fluent - Get all telephone numbers
        // - GET /api/telephone-numbers-fluent/{id} - Get telephone number by ID
        // - PUT /api/telephone-numbers-fluent/{id} - Update telephone number
        // - DELETE /api/telephone-numbers-fluent/{id} - Delete telephone number
        // - GET /api/telephone-numbers-fluent/customer/{customerId} - Get telephone numbers by customer
        //
        // HINTS:
        // 1. Inject IInvoiceRepository and ITelephoneNumberRepository in the constructor
        // 2. Create validators for Invoice and TelephoneNumber DTOs using FluentValidation
        // 3. Register validators in Program.cs
        // 4. Use the existing repository methods (CreateAsync, GetByIdAsync, UpdateAsync, DeleteAsync)
        // 5. Use mapping extensions (ToDto(), ToEntity(), UpdateEntity())
        // 6. Follow the same error handling pattern as Customer endpoints
        // 7. Validate invoice number uniqueness for invoices
        // 8. Ensure CustomerId exists before creating invoices or telephone numbers
    }
}
