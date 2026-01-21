using AZFuncCustInvTel.Models;
using AZFuncCustInvTel.DTOs;

namespace AZFuncCustInvTel.Mappings
{
    public static class MappingExtensions
    {
        // Customer mappings
        public static CustomerDto ToDto(this Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                Balance = customer.Balance,
                Invoices = customer.Invoices?.Select(i => i.ToDto()).ToList(),
                PhoneNumbers = customer.PhoneNumbers?.Select(p => p.ToDto()).ToList()
            };
        }

        public static Customer ToEntity(this CreateCustomerDto dto)
        {
            return new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Invoices = dto.Invoices?.Select(i => new Invoice
                {
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    Amount = i.Amount
                }).ToList() ?? new List<Invoice>(),
                PhoneNumbers = dto.PhoneNumbers?.Select(p => new TelephoneNumber
                {
                    Type = p.Type,
                    Number = p.Number
                }).ToList() ?? new List<TelephoneNumber>()
            };
        }

        public static void UpdateEntity(this UpdateCustomerDto dto, Customer customer)
        {
            customer.Name = dto.Name;
            customer.Email = dto.Email;
        }

        // Invoice mappings
        public static InvoiceDto ToDto(this Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                InvoiceDate = invoice.InvoiceDate,
                Amount = invoice.Amount
            };
        }

        public static Invoice ToEntity(this CreateInvoiceDto dto)
        {
            return new Invoice
            {
                InvoiceNumber = dto.InvoiceNumber,
                CustomerId = dto.CustomerId,
                InvoiceDate = dto.InvoiceDate,
                Amount = dto.Amount
            };
        }

        public static void UpdateEntity(this UpdateInvoiceDto dto, Invoice invoice)
        {
            invoice.InvoiceNumber = dto.InvoiceNumber;
            invoice.InvoiceDate = dto.InvoiceDate;
            invoice.Amount = dto.Amount;
        }

        // TelephoneNumber mappings
        public static TelephoneNumberDto ToDto(this TelephoneNumber telephoneNumber)
        {
            return new TelephoneNumberDto
            {
                Id = telephoneNumber.Id,
                CustomerId = telephoneNumber.CustomerId,
                Type = telephoneNumber.Type,
                Number = telephoneNumber.Number
            };
        }

        public static TelephoneNumber ToEntity(this CreateTelephoneNumberDto dto)
        {
            return new TelephoneNumber
            {
                CustomerId = dto.CustomerId,
                Type = dto.Type,
                Number = dto.Number
            };
        }

        public static void UpdateEntity(this UpdateTelephoneNumberDto dto, TelephoneNumber telephoneNumber)
        {
            telephoneNumber.Type = dto.Type;
            telephoneNumber.Number = dto.Number;
        }
    }
}
