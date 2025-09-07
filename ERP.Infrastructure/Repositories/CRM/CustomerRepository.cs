using ERP.Domain.CRM;
using ERP.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories.CRM;

public class CustomerRepository : ICustomerRepository
{
    private readonly ERPDbContext _context;

    public CustomerRepository(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(string status)
    {
        return await _context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders)
            .Where(c => c.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetByIndustryAsync(string industry)
    {
        return await _context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders)
            .Where(c => c.Industry == industry)
            .ToListAsync();
    }

    public async Task<Customer?> GetByCodeAsync(string customerCode)
    {
        return await _context.Customers
            .Include(c => c.Opportunities)
            .Include(c => c.Quotes)
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);
    }

    public async Task<Guid> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer.Id;
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}