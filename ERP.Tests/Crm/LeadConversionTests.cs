using ERP.Application.CRM;
using ERP.Domain.CRM;
using ERP.Infrastructure.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;

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
        var service = new LeadService(leadRepo, customerRepo, opportunityRepo, new FakeNumbers());

        var opportunityId = await service.ConvertToOpportunityAsync(leadId, tenantId, Guid.NewGuid());

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
        var service = new LeadService(leadRepo, customerRepo, opportunityRepo, new FakeNumbers());

        var customerId = await service.ConvertToCustomerAsync(leadId, tenantId, Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, customerId);
        Assert.Equal("Converted", lead.Status);
        Assert.Single(customerRepo.Created);
        Assert.Equal("John Smith", customerRepo.Created[0].CustomerName);
    }

    private sealed class FakeLeadRepository : ILeadRepository
    {
        private readonly Dictionary<Guid, Lead> _leads;

        public FakeLeadRepository(Lead lead) => _leads = new() { [lead.Id] = lead };

        public Task<Lead?> GetByIdAsync(Guid id, Guid tenantId)
        {
            if (tenantId == Guid.Empty) return Task.FromResult<Lead?>(null);
            return Task.FromResult(_leads.TryGetValue(id, out var lead) && lead.TenantId == tenantId ? lead : null);
        }

        public Task<IEnumerable<Lead>> GetAllAsync(Guid tenantId) =>
            Task.FromResult<IEnumerable<Lead>>(tenantId == Guid.Empty ? [] : _leads.Values.Where(l => l.TenantId == tenantId));
        public Task<IEnumerable<Lead>> GetBySourceAsync(string source, Guid tenantId) => GetAllAsync(tenantId);
        public Task<IEnumerable<Lead>> GetByStatusAsync(string status, Guid tenantId) => GetAllAsync(tenantId);
        public Task<IEnumerable<Lead>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId) => GetAllAsync(tenantId);
        public Task<IEnumerable<Lead>> SearchAsync(string searchTerm, Guid tenantId) => GetAllAsync(tenantId);
        public Task<Guid> CreateAsync(Lead lead) => throw new NotImplementedException();
        public Task<bool> UpdateAsync(Lead lead) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id, Guid tenantId) => Task.FromResult(true);
        public Task<IReadOnlyList<LeadDuplicateDto>> FindDuplicatesAsync(string? email, string? phone, Guid? excludeLeadId, Guid tenantId) =>
            Task.FromResult<IReadOnlyList<LeadDuplicateDto>>([]);
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

        public Task<Customer?> GetByIdAsync(Guid id, Guid tenantId) => Task.FromResult<Customer?>(null);
        public Task<IEnumerable<Customer>> GetAllAsync(Guid tenantId) => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<ERP.Shared.DTOs.PagedResult<Customer>> GetPagedAsync(
            ERP.Shared.DTOs.CRM.CustomerPagedQuery query, Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(new ERP.Shared.DTOs.PagedResult<Customer>());
        public Task<IReadOnlyDictionary<string, int>> GetDependentCountsAsync(
            Guid id, Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>());
        public Task<IEnumerable<Customer>> GetByStatusAsync(string status, Guid tenantId)
            => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<IEnumerable<Customer>> GetByIndustryAsync(string industry, Guid tenantId)
            => Task.FromResult(Enumerable.Empty<Customer>());
        public Task<Customer?> GetByCodeAsync(string customerCode, Guid tenantId) => Task.FromResult<Customer?>(null);
        public Task<bool> UpdateAsync(Customer customer) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id, Guid tenantId) => Task.FromResult(true);
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

        public Task<Opportunity?> GetByIdAsync(Guid id, Guid tenantId) => Task.FromResult<Opportunity?>(null);
        public Task<IEnumerable<Opportunity>> GetAllAsync(Guid tenantId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<bool> ExistsAsync(Guid id, Guid tenantId) => Task.FromResult(false);
        public Task<Opportunity?> GetByCodeAsync(string code, Guid tenantId) => Task.FromResult<Opportunity?>(null);
        public Task<bool> IsCodeUniqueAsync(string code, Guid tenantId, Guid? excludeId = null) => Task.FromResult(true);
        public Task<IEnumerable<Opportunity>> GetByCustomerAsync(Guid customerId, Guid tenantId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByStatusAsync(string status, Guid tenantId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByAssignedToAsync(Guid assignedTo, Guid tenantId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<IEnumerable<Opportunity>> GetByStageAsync(string stage, Guid tenantId) => Task.FromResult(Enumerable.Empty<Opportunity>());
        public Task<bool> UpdateAsync(Opportunity opportunity) => Task.FromResult(true);
        public Task<bool> DeleteAsync(Guid id, Guid tenantId) => Task.FromResult(true);
    }

    private sealed class FakeNumbers : ERP.Infrastructure.Numbering.IDocumentNumberService
    {
        private int _next;
        public Task<string> NextAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult($"{key}-{++_next}");
        public Task<IReadOnlyList<ERP.Shared.DTOs.Core.DocumentSequenceDto>> ListAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ERP.Shared.DTOs.Core.DocumentSequenceDto> UpdateAsync(string key, ERP.Shared.DTOs.Core.UpdateDocumentSequenceDto dto, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
