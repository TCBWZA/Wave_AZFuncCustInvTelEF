using AZFuncCustInvTel.Models;
using AZFuncCustInvTel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AZFuncCustInvTel.Tests
{
    [TestFixture]
    public class CustomerRepositoryIntegrationTests
    {
        private AppDbContext _context;
        private CustomerRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test
                .Options;

            _context = new AppDbContext(options);
            _repository = new CustomerRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region CreateAsync Tests

        [Test]
        public async Task CreateAsync_ValidCustomer_ReturnsCustomerWithId()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };

            // Act
            var result = await _repository.CreateAsync(customer);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Is.EqualTo(customer.Name));
            Assert.That(result.Email, Is.EqualTo(customer.Email));
        }

        [Test]
        public async Task CreateAsync_CustomerWithInvoicesAndPhones_CreatesAllRelatedEntities()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>
                {
                    new Invoice { InvoiceNumber = "INV-001", Amount = 100, InvoiceDate = DateTime.UtcNow },
                    new Invoice { InvoiceNumber = "INV-002", Amount = 200, InvoiceDate = DateTime.UtcNow }
                },
                PhoneNumbers = new List<TelephoneNumber>
                {
                    new TelephoneNumber { Type = "Mobile", Number = "555-1234" },
                    new TelephoneNumber { Type = "Work", Number = "555-5678" }
                }
            };

            // Act
            var result = await _repository.CreateAsync(customer);

            // Assert
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Invoices, Has.Count.EqualTo(2));
            Assert.That(result.PhoneNumbers, Has.Count.EqualTo(2));
            
            // Verify foreign keys are set
            Assert.That(result.Invoices!.All(i => i.CustomerId == result.Id), Is.True);
            Assert.That(result.PhoneNumbers!.All(p => p.CustomerId == result.Id), Is.True);
        }

        #endregion

        #region GetByIdAsync Tests

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsCustomer()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.GetByIdAsync(customer.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(customer.Id));
            Assert.That(result.Name, Is.EqualTo(customer.Name));
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WithIncludeRelated_ReturnsCustomerWithRelatedData()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>
                {
                    new Invoice { InvoiceNumber = "INV-001", Amount = 100, InvoiceDate = DateTime.UtcNow }
                },
                PhoneNumbers = new List<TelephoneNumber>
                {
                    new TelephoneNumber { Type = "Mobile", Number = "555-1234" }
                }
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.GetByIdAsync(customer.Id, includeRelated: true);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Invoices, Has.Count.EqualTo(1));
            Assert.That(result.PhoneNumbers, Has.Count.EqualTo(1));
        }

        #endregion

        #region GetAllAsync Tests

        [Test]
        public async Task GetAllAsync_MultipleCustomers_ReturnsAllCustomers()
        {
            // Arrange
            await _repository.CreateAsync(new Customer { Name = "Company 1", Email = "c1@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "Company 2", Email = "c2@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "Company 3", Email = "c3@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
        }

        [Test]
        public async Task GetAllAsync_NoCustomers_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region UpdateAsync Tests

        [Test]
        public async Task UpdateAsync_ExistingCustomer_UpdatesSuccessfully()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Old Name",
                Email = "old@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            customer.Name = "New Name";
            customer.Email = "new@company.com";
            var result = await _repository.UpdateAsync(customer);

            // Assert
            var updated = await _repository.GetByIdAsync(customer.Id);
            Assert.That(updated!.Name, Is.EqualTo("New Name"));
            Assert.That(updated.Email, Is.EqualTo("new@company.com"));
        }

        #endregion

        #region DeleteAsync Tests

        [Test]
        public async Task DeleteAsync_ExistingCustomer_ReturnsTrue()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.DeleteAsync(customer.Id);

            // Assert
            Assert.That(result, Is.True);
            
            var deleted = await _repository.GetByIdAsync(customer.Id);
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_NonExistingCustomer_ReturnsFalse()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region ExistsAsync Tests

        [Test]
        public async Task ExistsAsync_ExistingCustomer_ReturnsTrue()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.ExistsAsync(customer.Id);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ExistsAsync_NonExistingCustomer_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region EmailExistsAsync Tests

        [Test]
        public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.EmailExistsAsync("test@company.com");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
        {
            // Act
            var result = await _repository.EmailExistsAsync("notfound@company.com");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EmailExistsAsync_WithExcludeId_ExcludesSpecifiedCustomer()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.EmailExistsAsync("test@company.com", customer.Id);

            // Assert
            Assert.That(result, Is.False); // Should return false because we're excluding this customer
        }

        #endregion

        #region GetByEmailAsync Tests

        [Test]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsCustomer()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.GetByEmailAsync("test@company.com");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Email, Is.EqualTo("test@company.com"));
        }

        [Test]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByEmailAsync("notfound@company.com");

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region GetPagedAsync Tests

        [Test]
        public async Task GetPagedAsync_ReturnsCorrectPageAndTotalCount()
        {
            // Arrange - Create 15 customers
            for (int i = 1; i <= 15; i++)
            {
                await _repository.CreateAsync(new Customer
                {
                    Name = $"Company {i}",
                    Email = $"company{i}@test.com",
                    Invoices = new List<Invoice>(),
                    PhoneNumbers = new List<TelephoneNumber>()
                });
            }

            // Act - Get page 2 with 5 items per page
            var (items, totalCount) = await _repository.GetPagedAsync(page: 2, pageSize: 5);

            // Assert
            Assert.That(totalCount, Is.EqualTo(15));
            Assert.That(items, Has.Count.EqualTo(5));
        }

        #endregion

        #region SearchAsync Tests

        [Test]
        public async Task SearchAsync_ByName_ReturnsMatchingCustomers()
        {
            // Arrange
            await _repository.CreateAsync(new Customer { Name = "ABC Company", Email = "abc@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "XYZ Company", Email = "xyz@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "ABC Corp", Email = "corp@test.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });

            // Act
            var result = await _repository.SearchAsync(name: "ABC", email: null, minBalance: null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(c => c.Name.Contains("ABC")), Is.True);
        }

        [Test]
        public async Task SearchAsync_ByEmail_ReturnsMatchingCustomers()
        {
            // Arrange
            await _repository.CreateAsync(new Customer { Name = "Company 1", Email = "test@company.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "Company 2", Email = "info@company.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });
            await _repository.CreateAsync(new Customer { Name = "Company 3", Email = "test@business.com", Invoices = new List<Invoice>(), PhoneNumbers = new List<TelephoneNumber>() });

            // Act
            var result = await _repository.SearchAsync(name: null, email: "company.com", minBalance: null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
        }

        #endregion

        #region Balance Calculation Tests

        [Test]
        public async Task Balance_CalculatesCorrectlyFromInvoices()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>
                {
                    new Invoice { InvoiceNumber = "INV-001", Amount = 100, InvoiceDate = DateTime.UtcNow },
                    new Invoice { InvoiceNumber = "INV-002", Amount = 250, InvoiceDate = DateTime.UtcNow },
                    new Invoice { InvoiceNumber = "INV-003", Amount = 150, InvoiceDate = DateTime.UtcNow }
                },
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.GetByIdAsync(customer.Id, includeRelated: true);

            // Assert
            Assert.That(result!.Balance, Is.EqualTo(500));
        }

        [Test]
        public async Task Balance_NoInvoices_ReturnsZero()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "test@company.com",
                Invoices = new List<Invoice>(),
                PhoneNumbers = new List<TelephoneNumber>()
            };
            await _repository.CreateAsync(customer);

            // Act
            var result = await _repository.GetByIdAsync(customer.Id, includeRelated: true);

            // Assert
            Assert.That(result!.Balance, Is.EqualTo(0));
        }

        #endregion
    }
}
