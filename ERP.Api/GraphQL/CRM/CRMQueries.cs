using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using HotChocolate;

namespace ERP.Api.GraphQL.CRM;

[ExtendObjectType("Query")]
public class CRMQueries
{
    // Customer Queries
    public async Task<CustomerDto?> GetCustomer(
        [Service] ICustomerService customerService,
        Guid customerId)
    {
        return await customerService.GetByIdAsync(customerId);
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomers(
        [Service] ICustomerService customerService,
        string? status = null,
        string? industry = null)
    {
        var customers = await customerService.GetAllAsync();
        
        if (!string.IsNullOrEmpty(status))
            customers = customers.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(industry))
            customers = customers.Where(c => c.Industry == industry);
            
        return customers;
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomers(
        [Service] ICustomerService customerService,
        string searchTerm)
    {
        return await customerService.SearchAsync(searchTerm);
    }

    // Contact Queries
    public async Task<ContactDto?> GetContact(
        [Service] IContactService contactService,
        Guid contactId)
    {
        return await contactService.GetByIdAsync(contactId);
    }

    public async Task<IEnumerable<ContactDto>> GetContacts(
        [Service] IContactService contactService,
        Guid? customerId = null)
    {
        if (customerId.HasValue)
            return await contactService.GetByCustomerAsync(customerId.Value);

        return await contactService.GetAllAsync();
    }

    public async Task<IEnumerable<ContactDto>> SearchContacts(
        [Service] IContactService contactService,
        string searchTerm)
    {
        return await contactService.SearchAsync(searchTerm);
    }

    public async Task<IEnumerable<ContactDto>> GetContactsByCustomer(
        [Service] IContactService contactService,
        Guid customerId)
    {
        return await contactService.GetByCustomerAsync(customerId);
    }

    // Lead Queries
    public async Task<LeadDto?> GetLead(
        [Service] ILeadService leadService,
        Guid leadId)
    {
        return await leadService.GetByIdAsync(leadId);
    }

    public async Task<IEnumerable<LeadDto>> GetLeads(
        [Service] ILeadService leadService,
        string? source = null,
        string? status = null,
        Guid? assignedTo = null)
    {
        var leads = await leadService.GetAllAsync();
        
        if (!string.IsNullOrEmpty(source))
            leads = leads.Where(l => l.Source == source);
        if (!string.IsNullOrEmpty(status))
            leads = leads.Where(l => l.Status == status);
        if (assignedTo.HasValue)
            leads = leads.Where(l => l.AssignedTo == assignedTo.Value);
            
        return leads;
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsBySource(
        [Service] ILeadService leadService,
        string source)
    {
        return await leadService.GetBySourceAsync(source);
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsByStatus(
        [Service] ILeadService leadService,
        string status)
    {
        return await leadService.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsByAssignedTo(
        [Service] ILeadService leadService,
        Guid assignedTo)
    {
        return await leadService.GetByAssignedToAsync(assignedTo);
    }

    // Opportunity Queries
    public async Task<OpportunityDto?> GetOpportunity(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId)
    {
        return await opportunityService.GetByIdAsync(opportunityId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunities(
        [Service] IOpportunityService opportunityService,
        Guid? customerId = null,
        string? status = null,
        string? stage = null,
        Guid? assignedTo = null)
    {
        var opportunities = await opportunityService.GetAllAsync();
        
        if (customerId.HasValue)
            opportunities = opportunities.Where(o => o.CustomerId == customerId.Value);
        if (!string.IsNullOrEmpty(status))
            opportunities = opportunities.Where(o => o.Status == status);
        if (!string.IsNullOrEmpty(stage))
            opportunities = opportunities.Where(o => o.Stage == stage);
        if (assignedTo.HasValue)
            opportunities = opportunities.Where(o => o.AssignedTo == assignedTo.Value);
            
        return opportunities;
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByCustomer(
        [Service] IOpportunityService opportunityService,
        Guid customerId)
    {
        return await opportunityService.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByStatus(
        [Service] IOpportunityService opportunityService,
        string status)
    {
        return await opportunityService.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByStage(
        [Service] IOpportunityService opportunityService,
        string stage)
    {
        return await opportunityService.GetByStageAsync(stage);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByAssignedTo(
        [Service] IOpportunityService opportunityService,
        Guid assignedTo)
    {
        return await opportunityService.GetByAssignedToAsync(assignedTo);
    }

    // Quote Queries
    public async Task<QuoteDto?> GetQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId)
    {
        return await quoteService.GetByIdAsync(quoteId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotes(
        [Service] IQuoteService quoteService,
        Guid? customerId = null,
        string? status = null)
    {
        var quotes = await quoteService.GetAllAsync();
        
        if (customerId.HasValue)
            quotes = quotes.Where(q => q.CustomerId == customerId.Value);
        if (!string.IsNullOrEmpty(status))
            quotes = quotes.Where(q => q.Status == status);
            
        return quotes;
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByCustomer(
        [Service] IQuoteService quoteService,
        Guid customerId)
    {
        return await quoteService.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByStatus(
        [Service] IQuoteService quoteService,
        string status)
    {
        return await quoteService.GetByStatusAsync(status);
    }

    // Sales Order Queries
    public async Task<SalesOrderDto?> GetSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId)
    {
        return await salesOrderService.GetByIdAsync(salesOrderId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrders(
        [Service] ISalesOrderService salesOrderService,
        Guid? customerId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var salesOrders = await salesOrderService.GetAllAsync();
        
        if (customerId.HasValue)
            salesOrders = salesOrders.Where(so => so.CustomerId == customerId.Value);
        if (!string.IsNullOrEmpty(status))
            salesOrders = salesOrders.Where(so => so.Status == status);
        if (startDate.HasValue)
            salesOrders = salesOrders.Where(so => so.OrderDate >= startDate.Value);
        if (endDate.HasValue)
            salesOrders = salesOrders.Where(so => so.OrderDate <= endDate.Value);
            
        return salesOrders;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByCustomer(
        [Service] ISalesOrderService salesOrderService,
        Guid customerId)
    {
        return await salesOrderService.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByStatus(
        [Service] ISalesOrderService salesOrderService,
        string status)
    {
        return await salesOrderService.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByDateRange(
        [Service] ISalesOrderService salesOrderService,
        DateTime startDate,
        DateTime endDate)
    {
        return await salesOrderService.GetByDateRangeAsync(startDate, endDate);
    }

    // Activity Queries
    public async Task<ActivityDto?> GetActivity(
        [Service] IActivityService activityService,
        Guid activityId)
    {
        return await activityService.GetByIdAsync(activityId);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivities(
        [Service] IActivityService activityService,
        string? type = null,
        Guid? relatedToId = null,
        string? relatedToType = null,
        Guid? assignedTo = null)
    {
        var activities = await activityService.GetAllAsync();
        
        if (!string.IsNullOrEmpty(type))
            activities = activities.Where(a => a.ActivityType == type);
        if (relatedToId.HasValue)
            activities = activities.Where(a => a.RelatedToId == relatedToId.Value);
        if (!string.IsNullOrEmpty(relatedToType))
            activities = activities.Where(a => a.RelatedToType == relatedToType);
        if (assignedTo.HasValue)
            activities = activities.Where(a => a.AssignedToUserId == assignedTo.Value);
            
        return activities;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByType(
        [Service] IActivityService activityService,
        string type)
    {
        return await activityService.GetByTypeAsync(type);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByRelatedTo(
        [Service] IActivityService activityService,
        Guid relatedToId,
        string relatedToType)
    {
        return await activityService.GetByEntityAsync(relatedToType, relatedToId);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByAssignedTo(
        [Service] IActivityService activityService,
        Guid assignedTo)
    {
        return await activityService.GetByAssignedToAsync(assignedTo);
    }

    // Campaign Queries
    public async Task<CampaignDto?> GetCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId)
    {
        return await campaignService.GetByIdAsync(campaignId);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaigns(
        [Service] ICampaignService campaignService,
        string? status = null,
        string? campaignType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var campaigns = await campaignService.GetAllAsync();
        
        if (!string.IsNullOrEmpty(status))
            campaigns = campaigns.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(campaignType))
            campaigns = campaigns.Where(c => c.CampaignType == campaignType);
        if (startDate.HasValue)
            campaigns = campaigns.Where(c => c.StartDate >= startDate.Value);
        if (endDate.HasValue)
            campaigns = campaigns.Where(c => c.EndDate <= endDate.Value);
            
        return campaigns;
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsByStatus(
        [Service] ICampaignService campaignService,
        string status)
    {
        return await campaignService.GetByStatusAsync(status);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsByType(
        [Service] ICampaignService campaignService,
        string campaignType)
    {
        return await campaignService.GetByTypeAsync(campaignType);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsByDateRange(
        [Service] ICampaignService campaignService,
        DateTime startDate,
        DateTime endDate)
    {
        return await campaignService.GetByDateRangeAsync(startDate, endDate);
    }
}