using AZFuncCustInvTel.Models;

namespace AZFuncCustInvTel.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(long id, bool includeRelated = false);
        Task<Customer?> GetByEmailAsync(string email);
        Task<List<Customer>> GetAllAsync(bool includeRelated = false);
        Task<List<Customer>> GetAllWithSplitQueriesAsync();
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<bool> EmailExistsAsync(string email, long? excludeCustomerId = null);
        
        // Pagination support
        Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool includeRelated = false);
        
        // Search and filtering
        Task<List<Customer>> SearchAsync(string? name, string? email, decimal? minBalance);
        
        // Efficient read-only queries
        Task<List<Customer>> GetAllNoTrackingAsync();
    }

    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(long id);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<List<Invoice>> GetAllAsync();
        Task<List<Invoice>> GetByCustomerIdAsync(long customerId);
        Task<Invoice> CreateAsync(Invoice invoice);
        Task<Invoice> UpdateAsync(Invoice invoice);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, long? excludeInvoiceId = null);
    }

    public interface ITelephoneNumberRepository
    {
        Task<TelephoneNumber?> GetByIdAsync(long id);
        Task<List<TelephoneNumber>> GetAllAsync();
        Task<List<TelephoneNumber>> GetByCustomerIdAsync(long customerId);
        Task<TelephoneNumber> CreateAsync(TelephoneNumber telephoneNumber);
        Task<TelephoneNumber> UpdateAsync(TelephoneNumber telephoneNumber);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
    }
}
