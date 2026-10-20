using ERP.Application.CRM;
using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using HotChocolate;

namespace ERP.Api.GraphQL.CRM;

[ExtendObjectType("Mutation")]
public class CRMMutations
{
    public async Task<Guid> CreateCustomer(
        [Service] ICustomerService customerService,
        CreateCustomerDto input,
        Guid tenantId,
        Guid userId)
    {
        return await customerService.CreateAsync(input, tenantId, userId);
    }

    public async Task<bool> UpdateCustomer(
        [Service] ICustomerService customerService,
        UpdateCustomerDto input,
        Guid tenantId,
        Guid userId)
        => await customerService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteCustomer(
        [Service] ICustomerService customerService,
        Guid customerId,
        Guid tenantId)
        => await customerService.DeleteAsync(customerId, tenantId);

    public async Task<bool> ActivateCustomer(
        [Service] ICustomerService customerService,
        Guid customerId,
        Guid tenantId,
        Guid userId)
        => await customerService.ActivateAsync(customerId, tenantId, userId);

    public async Task<bool> DeactivateCustomer(
        [Service] ICustomerService customerService,
        Guid customerId,
        Guid tenantId,
        Guid userId)
        => await customerService.DeactivateAsync(customerId, tenantId, userId);

    public async Task<Guid> CreateContact(
        [Service] IContactService contactService,
        CreateContactDto input)
    {
        return await contactService.CreateAsync(input);
    }

    public async Task<bool> UpdateContact(
        [Service] IContactService contactService,
        UpdateContactDto input)
    {
        return await contactService.UpdateAsync(input);
    }

    public async Task<bool> DeleteContact(
        [Service] IContactService contactService,
        Guid contactId)
    {
        return await contactService.DeleteAsync(contactId);
    }

    public async Task<Guid> CreateLead(
        [Service] ILeadService leadService,
        CreateLeadDto input)
    {
        return await leadService.CreateAsync(input);
    }

    public async Task<bool> UpdateLead(
        [Service] ILeadService leadService,
        UpdateLeadDto input)
        => await leadService.UpdateAsync(input);

    public async Task<bool> DeleteLead(
        [Service] ILeadService leadService,
        Guid leadId)
        => await leadService.DeleteAsync(leadId);

    public async Task<Guid> CreateOpportunity(
        [Service] IOpportunityService opportunityService,
        CreateOpportunityDto input)
        => await opportunityService.CreateAsync(input);

    public async Task<bool> UpdateOpportunity(
        [Service] IOpportunityService opportunityService,
        UpdateOpportunityDto input)
        => await opportunityService.UpdateAsync(input);

    public async Task<bool> DeleteOpportunity(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId)
        => await opportunityService.DeleteAsync(opportunityId);

    public async Task<Guid> CreateQuote(
        [Service] IQuoteService quoteService,
        CreateQuoteDto input)
        => await quoteService.CreateAsync(input);

    public async Task<bool> UpdateQuote(
        [Service] IQuoteService quoteService,
        UpdateQuoteDto input)
        => await quoteService.UpdateAsync(input);

    public async Task<bool> DeleteQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId)
        => await quoteService.DeleteAsync(quoteId);

    public async Task<Guid> CreateSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        CreateSalesOrderDto input)
        => await salesOrderService.CreateAsync(input);

    public async Task<bool> UpdateSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        UpdateSalesOrderDto input)
        => await salesOrderService.UpdateAsync(input);

    public async Task<bool> DeleteSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId)
        => await salesOrderService.DeleteAsync(salesOrderId);

    public async Task<Guid> CreateActivity(
        [Service] IActivityService activityService,
        CreateActivityDto input)
        => await activityService.CreateAsync(input);

    public async Task<bool> UpdateActivity(
        [Service] IActivityService activityService,
        UpdateActivityDto input)
        => await activityService.UpdateAsync(input);

    public async Task<bool> DeleteActivity(
        [Service] IActivityService activityService,
        Guid activityId)
        => await activityService.DeleteAsync(activityId);

    public async Task<Guid> ConvertLeadToCustomer(
        [Service] ILeadService leadService,
        Guid leadId,
        Guid tenantId,
        Guid userId)
    {
        return await leadService.ConvertToCustomerAsync(leadId, tenantId, userId);
    }

    public async Task<Guid> ConvertLeadToOpportunity(
        [Service] ILeadService leadService,
        Guid leadId,
        Guid tenantId,
        Guid userId)
    {
        return await leadService.ConvertToOpportunityAsync(leadId, tenantId, userId);
    }

    public async Task<bool> QualifyLead(
        [Service] ILeadService leadService,
        Guid leadId,
        Guid tenantId,
        Guid userId)
    {
        return await leadService.QualifyAsync(leadId, tenantId, userId);
    }

    public async Task<bool> MarkLeadLost(
        [Service] ILeadService leadService,
        Guid leadId,
        Guid tenantId,
        Guid userId)
    {
        return await leadService.MarkLostAsync(leadId, tenantId, userId);
    }

    public async Task<Guid> ConvertOpportunityToQuote(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId,
        Guid tenantId,
        Guid userId)
    {
        return await opportunityService.ConvertToQuoteAsync(opportunityId, tenantId, userId);
    }

    public async Task<bool> MarkOpportunityWon(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId,
        Guid tenantId,
        Guid userId)
    {
        return await opportunityService.MarkWonAsync(opportunityId, tenantId, userId);
    }

    public async Task<bool> MarkOpportunityLost(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId,
        Guid tenantId,
        Guid userId,
        string? reason = null)
    {
        return await opportunityService.MarkLostAsync(opportunityId, tenantId, userId, reason);
    }

    public async Task<bool> CancelOpportunity(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId,
        Guid tenantId,
        Guid userId)
    {
        return await opportunityService.CancelAsync(opportunityId, tenantId, userId);
    }

    public async Task<Guid> ConvertQuoteToSalesOrder(
        [Service] IQuoteService quoteService,
        Guid quoteId,
        Guid tenantId,
        Guid userId)
    {
        return await quoteService.ConvertToSalesOrderAsync(quoteId, tenantId, userId);
    }

    public async Task<bool> ApproveQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId,
        Guid tenantId,
        Guid userId)
    {
        return await quoteService.ApproveAsync(quoteId, tenantId, userId);
    }

    public async Task<bool> RejectQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId,
        Guid tenantId,
        Guid userId)
    {
        return await quoteService.RejectAsync(quoteId, tenantId, userId);
    }

    public async Task<bool> SendQuote(
        [Service] IQuoteService quoteService,
        Guid quoteId,
        Guid tenantId,
        Guid userId)
    {
        return await quoteService.SendQuoteAsync(quoteId, tenantId, userId);
    }

    public async Task<bool> ApproveSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId,
        Guid tenantId,
        Guid userId)
    {
        return await salesOrderService.ApproveAsync(salesOrderId, tenantId, userId);
    }

    public async Task<bool> RejectSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId,
        Guid tenantId,
        Guid userId)
    {
        return await salesOrderService.RejectAsync(salesOrderId, tenantId, userId);
    }

    public async Task<bool> ShipSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId,
        Guid tenantId,
        Guid userId)
        => await salesOrderService.ShipAsync(salesOrderId, tenantId, userId);

    public async Task<bool> InvoiceSalesOrder(
        [Service] ISalesOrderService salesOrderService,
        Guid salesOrderId,
        Guid tenantId,
        Guid userId)
        => await salesOrderService.InvoiceAsync(salesOrderId, tenantId, userId);

    public async Task<bool> CompleteActivity(
        [Service] IActivityService activityService,
        Guid activityId,
        Guid tenantId,
        Guid userId)
    {
        return await activityService.CompleteAsync(activityId, tenantId, userId);
    }

    public async Task<bool> CancelActivity(
        [Service] IActivityService activityService,
        Guid activityId,
        Guid tenantId,
        Guid userId)
    {
        return await activityService.CancelAsync(activityId, tenantId, userId);
    }

    public async Task<bool> RescheduleActivity(
        [Service] IActivityService activityService,
        Guid activityId,
        DateTime newDate,
        Guid tenantId,
        Guid userId)
    {
        return await activityService.RescheduleAsync(activityId, newDate, tenantId, userId);
    }

    public async Task<Guid> CreateCampaign(
        [Service] ICampaignService campaignService,
        CreateCampaignDto input)
    {
        return await campaignService.CreateAsync(input);
    }

    public async Task<bool> UpdateCampaign(
        [Service] ICampaignService campaignService,
        UpdateCampaignDto input)
    {
        return await campaignService.UpdateAsync(input);
    }

    public async Task<bool> DeleteCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId)
        => await campaignService.DeleteAsync(campaignId);

    public async Task<bool> LaunchCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId,
        Guid tenantId,
        Guid userId)
    {
        return await campaignService.LaunchAsync(campaignId, tenantId, userId);
    }

    public async Task<bool> PauseCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId,
        Guid tenantId,
        Guid userId)
    {
        return await campaignService.PauseAsync(campaignId, tenantId, userId);
    }

    public async Task<bool> CompleteCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId,
        Guid tenantId,
        Guid userId)
    {
        return await campaignService.CompleteAsync(campaignId, tenantId, userId);
    }

    public async Task<bool> CancelCampaign(
        [Service] ICampaignService campaignService,
        Guid campaignId,
        Guid tenantId,
        Guid userId)
    {
        return await campaignService.CancelAsync(campaignId, tenantId, userId);
    }

    // Return Order — Pending → Approved → Completed / Cancelled
    public async Task<Guid> CreateReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        CreateReturnOrderDto input,
        Guid tenantId,
        Guid userId)
        => await returnOrderService.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        UpdateReturnOrderDto input,
        Guid tenantId,
        Guid userId)
        => await returnOrderService.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        Guid returnOrderId,
        Guid tenantId)
        => await returnOrderService.DeleteAsync(returnOrderId, tenantId);

    public async Task<bool> ApproveReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        Guid returnOrderId,
        Guid tenantId,
        Guid userId)
        => await returnOrderService.ApproveAsync(returnOrderId, tenantId, userId);

    public async Task<bool> CompleteReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        Guid returnOrderId,
        Guid tenantId,
        Guid userId)
        => await returnOrderService.CompleteAsync(returnOrderId, tenantId, userId);

    public async Task<bool> CancelReturnOrder(
        [Service] IReturnOrderService returnOrderService,
        Guid returnOrderId,
        Guid tenantId,
        Guid userId)
        => await returnOrderService.CancelAsync(returnOrderId, tenantId, userId);

    // Extended CRM masters
    public async Task<Guid> AddSalesman(
        [Service] ISalesmanService service, CreateSalesmanDto input, Guid tenantId, Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateSalesman(
        [Service] ISalesmanService service, UpdateSalesmanDto input, Guid tenantId, Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteSalesman(
        [Service] ISalesmanService service, Guid salesmanId, Guid tenantId)
        => await service.DeleteAsync(salesmanId, tenantId);

    public async Task<Guid> AddCustomerGroup(
        [Service] ICustomerGroupService service, CreateCustomerGroupDto input, Guid tenantId, Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateCustomerGroup(
        [Service] ICustomerGroupService service, UpdateCustomerGroupDto input, Guid tenantId, Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteCustomerGroup(
        [Service] ICustomerGroupService service, Guid groupId, Guid tenantId)
        => await service.DeleteAsync(groupId, tenantId);

    public async Task<Guid> AddPriceList(
        [Service] IPriceListService service, CreatePriceListDto input, Guid tenantId, Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdatePriceList(
        [Service] IPriceListService service, UpdatePriceListDto input, Guid tenantId, Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeletePriceList(
        [Service] IPriceListService service, Guid priceListId, Guid tenantId)
        => await service.DeleteAsync(priceListId, tenantId);

    public async Task<Guid> AddSalesTerritory(
        [Service] ISalesTerritoryService service, CreateSalesTerritoryDto input, Guid tenantId, Guid userId)
        => await service.CreateAsync(input, tenantId, userId);

    public async Task<bool> UpdateSalesTerritory(
        [Service] ISalesTerritoryService service, UpdateSalesTerritoryDto input, Guid tenantId, Guid userId)
        => await service.UpdateAsync(input, tenantId, userId);

    public async Task<bool> DeleteSalesTerritory(
        [Service] ISalesTerritoryService service, Guid territoryId, Guid tenantId)
        => await service.DeleteAsync(territoryId, tenantId);
}
