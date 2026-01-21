using Microsoft.EntityFrameworkCore;

namespace AZFuncCustInvTel.Models
{
    // Data/AppDbContext.cs

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<TelephoneNumber> TelephoneNumbers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).ValueGeneratedOnAdd();
                b.Property(c => c.Name).HasMaxLength(200);
                b.Property(c => c.Email).HasMaxLength(200);
                b.HasMany(c => c.Invoices).WithOne().HasForeignKey(i => i.CustomerId);
                b.HasMany(c => c.PhoneNumbers).WithOne().HasForeignKey(t => t.CustomerId);
                b.Ignore(c => c.Balance);
                b.HasIndex(c => c.Email).IsUnique();
            });

            modelBuilder.Entity<Invoice>(b =>
            {
                b.HasKey(i => i.Id);
                b.Property(i => i.Id).ValueGeneratedOnAdd();
                b.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
                b.Property(i => i.Amount).HasColumnType("decimal(18,2)");
                b.Property(i => i.InvoiceDate).IsRequired();
                b.Property(i => i.CustomerId).IsRequired();
                b.HasIndex(i => i.InvoiceNumber).IsUnique();
                b.ToTable(t => t.HasCheckConstraint("CK_Invoice_Amount", "Amount >= 0"));
            });

            modelBuilder.Entity<TelephoneNumber>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Id).ValueGeneratedOnAdd();
                b.Property(t => t.Number).HasMaxLength(50);
                b.Property(t => t.Type).HasMaxLength(20);
                b.Property(t => t.CustomerId).IsRequired();
                b.ToTable(t => t.HasCheckConstraint("CK_TelephoneNumber_Type", "Type IN ('Mobile', 'Work', 'DirectDial')"));
            });
        }
    }
}
