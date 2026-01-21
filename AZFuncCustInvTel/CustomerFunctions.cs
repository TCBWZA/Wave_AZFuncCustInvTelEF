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
    public class CustomerFunctions
    {
        private readonly ILogger<CustomerFunctions> _logger;
        private readonly ICustomerRepository _customerRepository;

        public CustomerFunctions(ILogger<CustomerFunctions> logger, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
        }

        [Function("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
        {
            _logger.LogInformation("CreateCustomer called.");

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

            var validationErrors = ValidateModel(dto);
            if (validationErrors.Any())
            {
                return new BadRequestObjectResult(new { errors = validationErrors });
            }

            if (await _customerRepository.EmailExistsAsync(dto.Email))
            {
                return new BadRequestObjectResult(new { error = $"A customer with email '{dto.Email}' already exists." });
            }

            var customer = dto.ToEntity();
            var createdCustomer = await _customerRepository.CreateAsync(customer);
            var customerDto = createdCustomer.ToDto();

            return new CreatedResult($"/api/customers/{customerDto.Id}", customerDto);
        }

        [Function("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("UpdateCustomer called for id {Id}.", id);

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

            var validationErrors = ValidateModel(dto);
            if (validationErrors.Any())
            {
                return new BadRequestObjectResult(new { errors = validationErrors });
            }

            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            if (await _customerRepository.EmailExistsAsync(dto.Email, id))
            {
                return new BadRequestObjectResult(new { error = $"A customer with email '{dto.Email}' already exists." });
            }

            dto.UpdateEntity(existingCustomer);
            var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
            var customerDto = updatedCustomer.ToDto();

            return new OkObjectResult(customerDto);
        }

        [Function("GetCustomer")]
        public async Task<IActionResult> GetCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("GetCustomer called for id {Id}.", id);

            var customer = await _customerRepository.GetByIdAsync(id, includeRelated: true);
            if (customer == null)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            var customerDto = customer.ToDto();
            return new OkObjectResult(customerDto);
        }

        [Function("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllCustomers called.");

            var customers = await _customerRepository.GetAllAsync(includeRelated: true);
            var customerDtos = customers.Select(c => c.ToDto()).ToList();

            return new OkObjectResult(customerDtos);
        }

        [Function("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("DeleteCustomer called for id {Id}.", id);

            var deleted = await _customerRepository.DeleteAsync(id);
            if (!deleted)
            {
                return new NotFoundObjectResult(new { error = $"Customer with id {id} not found." });
            }

            return new OkObjectResult(new { message = $"Customer with id {id} deleted successfully." });
        }

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
    }
}
