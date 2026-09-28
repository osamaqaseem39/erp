using ERP.Application.Interfaces.CRM;
using ERP.Shared.DTOs.CRM;
using HotChocolate;

namespace ERP.Api.GraphQL.CRM;

[ExtendObjectType("Mutation")]
public class CRMMutations
{
    public async Task<Guid> CreateCustomer(
        [Service] ICustomerService customerService,
        CreateCustomerDto input)
    {
        return await customerService.CreateAsync(input);
    }

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

    public async Task<Guid> ConvertLeadToCustomer(
        [Service] ILeadService leadService,
        Guid leadId)
    {
        return await leadService.ConvertToCustomerAsync(leadId);
    }

    public async Task<Guid> ConvertLeadToOpportunity(
        [Service] ILeadService leadService,
        Guid leadId)
    {
        return await leadService.ConvertToOpportunityAsync(leadId);
    }

    public async Task<Guid> ConvertOpportunityToQuote(
        [Service] IOpportunityService opportunityService,
        Guid opportunityId)
    {
        return await opportunityService.ConvertToQuoteAsync(opportunityId);
    }

    public async Task<Guid> ConvertQuoteToSalesOrder(
        [Service] IQuoteService quoteService,
        Guid quoteId)
    {
        return await quoteService.ConvertToSalesOrderAsync(quoteId);
    }

    public async Task<bool> CompleteActivity(
        [Service] IActivityService activityService,
        Guid activityId)
    {
        return await activityService.CompleteAsync(activityId);
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
}
