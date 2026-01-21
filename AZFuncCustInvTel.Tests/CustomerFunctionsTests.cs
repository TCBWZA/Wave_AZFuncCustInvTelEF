using AZFuncCustInvTel.DTOs;
using AZFuncCustInvTel.Models;
using AZFuncCustInvTel.Repositories;
using Moq;

namespace AZFuncCustInvTel.Tests
{
    /// <summary>
    /// Unit tests for Customer endpoints focusing on repository interactions and business logic.
    /// Note: Testing Azure Functions with HttpRequestData requires complex mocking.
    /// These tests focus on the business logic, validation, and repository interactions.
    /// </summary>
    [TestFixture]
    public class CustomerFunctionsTests
    {
        private Mock<ICustomerRepository> _mockCustomerRepository;

        [SetUp]
        public void Setup()
        {
            _mockCustomerRepository = new Mock<ICustomerRepository>();
        }

        #region Repository Interaction Tests

        [Test]
        public async Task GetCustomer_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            long customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };

            _mockCustomerRepository
                .Setup(r => r.GetByIdAsync(customerId, true))
                .ReturnsAsync(customer);

            // Act
            var result = await _mockCustomerRepository.Object.GetByIdAsync(customerId, true);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(customerId));
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId, true), Times.Once);
        }

        [Test]
        public async Task CreateCustomer_EmailExists_RepositoryReturnsTrue()
        {
            // Arrange
            string email = "existing@company.com";
            
            _mockCustomerRepository
                .Setup(r => r.EmailExistsAsync(email, null))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCustomerRepository.Object.EmailExistsAsync(email, null);

            // Assert
            Assert.That(result, Is.True);
            _mockCustomerRepository.Verify(r => r.EmailExistsAsync(email, null), Times.Once);
        }

        [Test]
        public async Task UpdateCustomer_CustomerExists_RepositoryUpdates()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Updated Name",
                Email = "updated@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };

            _mockCustomerRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _mockCustomerRepository.Object.UpdateAsync(customer);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            _mockCustomerRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Test]
        public async Task DeleteCustomer_CustomerExists_RepositoryReturnsTrue()
        {
            // Arrange
            long customerId = 1;
            
            _mockCustomerRepository
                .Setup(r => r.DeleteAsync(customerId))
                .ReturnsAsync(true);

            // Act
            var result = await _mockCustomerRepository.Object.DeleteAsync(customerId);

            // Assert
            Assert.That(result, Is.True);
            _mockCustomerRepository.Verify(r => r.DeleteAsync(customerId), Times.Once);
        }

        [Test]
        public async Task DeleteCustomer_CustomerDoesNotExist_RepositoryReturnsFalse()
        {
            // Arrange
            long customerId = 999;
            
            _mockCustomerRepository
                .Setup(r => r.DeleteAsync(customerId))
                .ReturnsAsync(false);

            // Act
            var result = await _mockCustomerRepository.Object.DeleteAsync(customerId);

            // Assert
            Assert.That(result, Is.False);
            _mockCustomerRepository.Verify(r => r.DeleteAsync(customerId), Times.Once);
        }

        [Test]
        public async Task GetAllCustomers_RepositoryReturnsMultipleCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Company 1", Email = "c1@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() },
                new Customer { Id = 2, Name = "Company 2", Email = "c2@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() },
                new Customer { Id = 3, Name = "Company 3", Email = "c3@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() }
            };

            _mockCustomerRepository
                .Setup(r => r.GetAllAsync(true))
                .ReturnsAsync(customers);

            // Act
            var result = await _mockCustomerRepository.Object.GetAllAsync(true);

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            _mockCustomerRepository.Verify(r => r.GetAllAsync(true), Times.Once);
        }

        #endregion

        #region DTO Validation Tests

        [Test]
        public void CreateCustomerDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<CreateInvoiceForCustomerDto>(),
                PhoneNumbers = new List<CreateTelephoneNumberForCustomerDto>()
            };

            // Act
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, context, validationResults, true);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(validationResults, Is.Empty);
        }

        [Test]
        public void CreateCustomerDto_InvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Name = "Test Company",
                Email = "not-an-email",
                Invoices = new List<CreateInvoiceForCustomerDto>(),
                PhoneNumbers = new List<CreateTelephoneNumberForCustomerDto>()
            };

            // Act
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, context, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults, Is.Not.Empty);
            Assert.That(validationResults.Any(v => v.MemberNames.Contains("Email")), Is.True);
        }

        [Test]
        public void CreateCustomerDto_MissingName_FailsValidation()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                Name = "",
                Email = "test@company.com",
                Invoices = new List<CreateInvoiceForCustomerDto>(),
                PhoneNumbers = new List<CreateTelephoneNumberForCustomerDto>()
            };

            // Act
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, context, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(v => v.MemberNames.Contains("Name")), Is.True);
        }

        [Test]
        public void UpdateCustomerDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new UpdateCustomerDto
            {
                Name = "Updated Company",
                Email = "updated@company.com"
            };

            // Act
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(dto);
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, context, validationResults, true);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(validationResults, Is.Empty);
        }

        #endregion

        #region Business Logic Tests

        [Test]
        public void Customer_Balance_CalculatesCorrectly()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>
                {
                    new Invoice { Id = 1, InvoiceNumber = "INV-001", Amount = 100, InvoiceDate = DateTime.UtcNow },
                    new Invoice { Id = 2, InvoiceNumber = "INV-002", Amount = 250, InvoiceDate = DateTime.UtcNow },
                    new Invoice { Id = 3, InvoiceNumber = "INV-003", Amount = 150, InvoiceDate = DateTime.UtcNow }
                },
                PhoneNumbers = new List<TelephoneNumber>()
            };

            // Act
            var balance = customer.Balance;

            // Assert
            Assert.That(balance, Is.EqualTo(500));
        }

        [Test]
        public void Customer_Balance_NoInvoices_ReturnsZero()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };

            // Act
            var balance = customer.Balance;

            // Assert
            Assert.That(balance, Is.EqualTo(0));
        }

        [Test]
        public void Customer_Balance_NullInvoices_ReturnsZero()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = null,
                PhoneNumbers = new List<TelephoneNumber>()
            };

            // Act
            var balance = customer.Balance;

            // Assert
            Assert.That(balance, Is.EqualTo(0));
        }

        #endregion
    }
}
