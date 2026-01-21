using AZFuncCustInvTel.Models;
using Microsoft.EntityFrameworkCore;

namespace AZFuncCustInvTel.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(long id)
        {
            return await _context.Invoices.FindAsync(id);
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices.ToListAsync();
        }

        public async Task<List<Invoice>> GetByCustomerIdAsync(long customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.Invoices.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, long? excludeInvoiceId = null)
        {
            var query = _context.Invoices.Where(i => i.InvoiceNumber == invoiceNumber);
            
            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(i => i.Id != excludeInvoiceId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
