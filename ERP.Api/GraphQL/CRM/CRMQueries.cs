using ERP.Application.CRM;
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
        Guid customerId,
        Guid tenantId)
    {
        return await customerService.GetByIdAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomers(
        [Service] ICustomerService customerService,
        Guid tenantId,
        string? status = null,
        string? industry = null)
    {
        if (!string.IsNullOrEmpty(status))
            return await customerService.GetByStatusAsync(status, tenantId);
        if (!string.IsNullOrEmpty(industry))
            return await customerService.GetByIndustryAsync(industry, tenantId);
        return await customerService.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomers(
        [Service] ICustomerService customerService,
        string searchTerm,
        Guid tenantId)
    {
        return await customerService.SearchAsync(searchTerm, tenantId);
    }

    // Contact Queries
    public async Task<ContactDto?> GetContact(
        [Service] IContactService contactService,
        Guid contactId,
        Guid tenantId)
        => await contactService.GetByIdAsync(contactId, tenantId);

    public async Task<IEnumerable<ContactDto>> GetContacts(
        [Service] IContactService contactService,
        Guid tenantId,
        Guid? customerId = null)
    {
        if (customerId.HasValue)
            return await contactService.GetByCustomerAsync(customerId.Value, tenantId);
        return await contactService.GetAllAsync(tenantId);
    }

    public async Task<IEnumerable<ContactDto>> SearchContacts(
        [Service] IContactService contactService,
        string searchTerm,
        Guid tenantId)
        => await contactService.SearchAsync(searchTerm, tenantId);

    public async Task<IEnumerable<ContactDto>> GetContactsByCustomer(
        [Service] IContactService contactService,
        Guid customerId,
        Guid tenantId)
        => await contactService.GetByCustomerAsync(customerId, tenantId);

    // Lead Queries
    public async Task<LeadDto?> GetLead(
        [Service] ILeadService leadService,
        Guid leadId,
        Guid tenantId)
    {
        return await leadService.GetByIdAsync(leadId, tenantId);
    }

    public async Task<IEnumerable<LeadDto>> GetLeads(
        [Service] ILeadService leadService,
        Guid tenantId,
        string? source = null,
        string? status = null,
        Guid? assignedTo = null)
    {
        IEnumerable<LeadDto> leads;
        if (!string.IsNullOrEmpty(source))
            leads = await leadService.GetBySourceAsync(source, tenantId);
        else if (!string.IsNullOrEmpty(status))
            leads = await leadService.GetByStatusAsync(status, tenantId);
        else
            leads = await leadService.GetAllAsync(tenantId);

        if (!string.IsNullOrEmpty(status) && string.IsNullOrEmpty(source))
            return leads;
        if (!string.IsNullOrEmpty(status))
            leads = leads.Where(l => l.Status == status);
        if (assignedTo.HasValue)
            leads = leads.Where(l => l.AssignedTo == assignedTo.Value);

        return leads;
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsBySource(
        [Service] ILeadService leadService,
        string source,
        Guid tenantId)
    {
        return await leadService.GetBySourceAsync(source, tenantId);
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsByStatus(
        [Service] ILeadService leadService,
        string status,
        Guid tenantId)
    {
        return await leadService.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<LeadDto>> GetLeadsByAssignedTo(
        [Service] ILeadService leadService,
        Guid assignedTo,
        Guid tenantId)
    {
        return await leadService.GetByAssignedToAsync(assignedTo, tenantId);
    }

    // Opportunity Queries
    public async Task<OpportunityDto?> GetOpportunity(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId,
        Guid tenantId)
    {
        return await opportunityService.GetByIdAsync(opportunityId, tenantId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunities(
        [Service] IOpportunityService opportunityService,
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        string? stage = null,
        Guid? assignedTo = null)
    {
        var opportunities = await opportunityService.GetAllAsync(tenantId);

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
        Guid customerId,
        Guid tenantId)
    {
        return await opportunityService.GetByCustomerAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByStatus(
        [Service] IOpportunityService opportunityService,
        string status,
        Guid tenantId)
    {
        return await opportunityService.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByStage(
        [Service] IOpportunityService opportunityService,
        string stage,
        Guid tenantId)
    {
        return await opportunityService.GetByStageAsync(stage, tenantId);
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesByAssignedTo(
        [Service] IOpportunityService opportunityService,
        Guid assignedTo,
        Guid tenantId)
    {
        return await opportunityService.GetByAssignedToAsync(assignedTo, tenantId);
    }

    // Quote Queries
    public async Task<QuoteDto?> GetQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId,
        Guid tenantId)
    {
        return await quoteService.GetByIdAsync(quoteId, tenantId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotes(
        [Service] IQuoteService quoteService,
        Guid tenantId,
        Guid? customerId = null,
        string? status = null)
    {
        var quotes = await quoteService.GetAllAsync(tenantId);

        if (customerId.HasValue)
            quotes = quotes.Where(q => q.CustomerId == customerId.Value);
        if (!string.IsNullOrEmpty(status))
            quotes = quotes.Where(q => q.Status == status);

        return quotes;
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByCustomer(
        [Service] IQuoteService quoteService,
        Guid customerId,
        Guid tenantId)
    {
        return await quoteService.GetByCustomerAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<QuoteDto>> GetQuotesByStatus(
        [Service] IQuoteService quoteService,
        string status,
        Guid tenantId)
    {
        return await quoteService.GetByStatusAsync(status, tenantId);
    }

    // Sales Order Queries
    public async Task<SalesOrderDto?> GetSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId,
        Guid tenantId)
    {
        return await salesOrderService.GetByIdAsync(salesOrderId, tenantId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrders(
        [Service] ISalesOrderService salesOrderService,
        Guid tenantId,
        Guid? customerId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var salesOrders = await salesOrderService.GetAllAsync(tenantId);

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
        Guid customerId,
        Guid tenantId)
    {
        return await salesOrderService.GetByCustomerAsync(customerId, tenantId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByStatus(
        [Service] ISalesOrderService salesOrderService,
        string status,
        Guid tenantId)
    {
        return await salesOrderService.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<SalesOrderDto>> GetSalesOrdersByDateRange(
        [Service] ISalesOrderService salesOrderService,
        DateTime startDate,
        DateTime endDate,
        Guid tenantId)
    {
        return await salesOrderService.GetByDateRangeAsync(startDate, endDate, tenantId);
    }

    // Activity Queries
    public async Task<ActivityDto?> GetActivity(
        [Service] IActivityService activityService,
        Guid activityId,
        Guid tenantId)
    {
        return await activityService.GetByIdAsync(activityId, tenantId);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivities(
        [Service] IActivityService activityService,
        Guid tenantId,
        string? type = null,
        Guid? relatedToId = null,
        string? relatedToType = null,
        Guid? assignedTo = null)
    {
        var activities = await activityService.GetAllAsync(tenantId);

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
        string type,
        Guid tenantId)
    {
        return await activityService.GetByTypeAsync(type, tenantId);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByRelatedTo(
        [Service] IActivityService activityService,
        Guid relatedToId,
        string relatedToType,
        Guid tenantId)
    {
        return await activityService.GetByEntityAsync(relatedToType, relatedToId, tenantId);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByAssignedTo(
        [Service] IActivityService activityService,
        Guid assignedTo,
        Guid tenantId)
    {
        return await activityService.GetByAssignedToAsync(assignedTo, tenantId);
    }

    // Campaign Queries
    public async Task<CampaignDto?> GetCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId,
        Guid tenantId)
    {
        return await campaignService.GetByIdAsync(campaignId, tenantId);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaigns(
        [Service] ICampaignService campaignService,
        Guid tenantId,
        string? status = null,
        string? campaignType = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var campaigns = await campaignService.GetAllAsync(tenantId);

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
        string status,
        Guid tenantId)
    {
        return await campaignService.GetByStatusAsync(status, tenantId);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsByType(
        [Service] ICampaignService campaignService,
        string campaignType,
        Guid tenantId)
    {
        return await campaignService.GetByTypeAsync(campaignType, tenantId);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsByDateRange(
        [Service] ICampaignService campaignService,
        DateTime startDate,
        DateTime endDate,
        Guid tenantId)
    {
        return await campaignService.GetByDateRangeAsync(startDate, endDate, tenantId);
    }

    // Return Order Queries
    public async Task<ReturnOrderDto?> GetReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        Guid returnOrderId,
        Guid tenantId)
        => await returnOrderService.GetByIdAsync(returnOrderId, tenantId);

    public async Task<IEnumerable<ReturnOrderDto>> GetReturnOrders(
        [Service] IReturnOrderService returnOrderService,
        Guid tenantId,
        string? status = null)
    {
        var orders = await returnOrderService.GetAllAsync(tenantId);
        if (string.IsNullOrEmpty(status)) return orders;
        return orders.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
    }

    // Extended CRM masters
    public async Task<SalesmanDto?> GetSalesman(
        [Service] ISalesmanService service, Guid salesmanId, Guid tenantId)
        => await service.GetByIdAsync(salesmanId, tenantId);

    public async Task<IEnumerable<SalesmanDto>> GetSalesmen(
        [Service] ISalesmanService service, Guid tenantId)
        => await service.GetAllAsync(tenantId);

    public async Task<CustomerGroupDto?> GetCustomerGroup(
        [Service] ICustomerGroupService service, Guid groupId, Guid tenantId)
        => await service.GetByIdAsync(groupId, tenantId);

    public async Task<IEnumerable<CustomerGroupDto>> GetCustomerGroups(
        [Service] ICustomerGroupService service, Guid tenantId)
        => await service.GetAllAsync(tenantId);

    public async Task<PriceListDto?> GetPriceList(
        [Service] IPriceListService service, Guid priceListId, Guid tenantId)
        => await service.GetByIdAsync(priceListId, tenantId);

    public async Task<IEnumerable<PriceListDto>> GetPriceLists(
        [Service] IPriceListService service, Guid tenantId)
        => await service.GetAllAsync(tenantId);

    public async Task<SalesTerritoryDto?> GetSalesTerritory(
        [Service] ISalesTerritoryService service, Guid territoryId, Guid tenantId)
        => await service.GetByIdAsync(territoryId, tenantId);

    public async Task<IEnumerable<SalesTerritoryDto>> GetSalesTerritories(
        [Service] ISalesTerritoryService service, Guid tenantId)
        => await service.GetAllAsync(tenantId);
}
