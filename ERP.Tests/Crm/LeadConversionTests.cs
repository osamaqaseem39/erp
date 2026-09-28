using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;

namespace ERP.Tests.Crm;

public class LeadConversionTests
{
    [Fact]
    public async Task ConvertToOpportunityAsync_CreatesOpportunityAndMarksLeadConverted()
    {
        var leadId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lead = new Lead
        {
            Id = leadId,
            TenantId = tenantId,
            FirstName = "Jane",
            LastName = "Doe",
            CompanyName = "Acme",
            Source = "Web",
            EstimatedValue = 5000m,
            Status = "Qualified"
        };

        var leadRepo = new FakeLeadRepository(lead);
        var customerRepo = new FakeCustomerRepository();
        var opportunityRepo = new FakeOpportunityRepository();
        var service = new LeadService(leadRepo, customerRepo, opportunityRepo);

        var opportunityId = await service.ConvertToOpportunityAsync(leadId);

        Assert.NotEqual(Guid.Empty, opportunityId);
        Assert.Equal("Converted", lead.Status);
        Assert.NotNull(lead.ConvertedDate);
        Assert.Single(opportunityRepo.Created);
        Assert.Equal(leadId, opportunityRepo.Created[0].LeadId);
        Assert.Equal(5000m, opportunityRepo.Created[0].Amount);
    }

    [Fact]
    public async Task ConvertToCustomerAsync_CreatesCustomerAndMarksLeadConverted()
    {
        var leadId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var lead = new Lead
        {
            Id = leadId,
            TenantId = tenantId,
            FirstName = "John",
            LastName = "Smith",
            Source = "Referral",
            Status = "Qualified"
        };

        var leadRepo = new FakeLeadRepository(lead);
        var customerRepo = new FakeCustomerRepository();
        var opportunityRepo = new FakeOpportunityRepository();
        var service = new LeadService(leadRepo, customerRepo, opportunityRepo);

        var customerId = await service.ConvertToCustomerAsync(leadId);

        Assert.NotEqual(Guid.Empty, customerId);
        Assert.Equal("Converted", lead.Status);
        Assert.Single(customerRepo.Created);
        Assert.Equal("John Smith", customerRepo.Created[0].CustomerName);
    }

    private sealed class FakeLeadRepository : ILeadRepository
    {
        private readonly Dictionary<Guid, Lead> _leads;

        public FakeLeadRepository(Lead lead) => _leads = new() { [lead.Id] = lead };

        public Task<Lead?> GetByIdAsync(Guid id) =>
            Task.FromResult(_leads.TryGetValue(id, out var lead) ? lead : null);

        public Task<IEnumerable<Lead>> GetAllAsync() => Task.FromResult<IEnumerable<Lead>>(_leads.Values);
        public Task<IEnumerable<Lead>> GetBySourceAsync(string source) => GetAllAsync();
        public Task<IEnumerable<Lead>> GetByStatusAsync(string status) => GetAllAsync();
        public Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedTo) => GetAllAsync();
        public Task<IEnumerable<Lead>> SearchAsync(string searchTerm) => GetAllAsync();
        public Task<Guid> CreateAsync(Lead lead) => throw new NotImplementedException();
        public Task<bool> UpdateAsync(Lead lead) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Created { get; } = new();

        public Task<Guid> CreateAsync(Customer customer)
        {
            customer.Id = Guid.NewGuid();
            Created.Add(customer);
            return Task.FromResult(customer.Id);
        }

        public Task<Customer?> GetByIdAsync(Guid id) => Task.FromResult<Customer?>(null);
        public Task<IEnumerable<Customer>> GetAllAsync() => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<IEnumerable<Customer>> GetByStatusAsync(string status) => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<IEnumerable<Customer>> GetByIndustryAsync(string industry) => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<Customer?> GetByCodeAsync(string customerCode) => Task.FromResult<Customer?>(null);
        public Task<bool> UpdateAsync(Customer customer) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class FakeOpportunityRepository : IOpportunityRepository
    {
        public List<Opportunity> Created { get; } = new();

        public Task<Guid> CreateAsync(Opportunity opportunity)
        {
            opportunity.Id = Guid.NewGuid();
            Created.Add(opportunity);
            return Task.FromResult(opportunity.Id);
        }

        public Task<Opportunity?> GetByIdAsync(Guid id) => Task.FromResult<Opportunity?>(null);
        public Task<IEnumerable<Opportunity>> GetAllAsync() => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<bool> ExistsAsync(Guid id) => Task.FromResult(false);
        public Task<Opportunity?> GetByCodeAsync(string code) => Task.FromResult<Opportunity?>(null);
        public Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null) => Task.FromResult(true);
        public Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByStatusAsync(string status) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByStageAsync(string stage) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<bool> UpdateAsync(Opportunity opportunity) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id) => Task.FromResult(true);
    }
}
