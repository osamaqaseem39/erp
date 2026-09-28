import { APICore } from '../../../../../helpers/api/apiCore';
import type { QuoteDto, CreateQuoteDto, UpdateQuoteDto } from '../types';

const api = new APICore();

class QuoteService {
  /**
   * Get all quotes
   */
  getAll = async (): Promise<QuoteDto[]> => {
    const response = await api.get('/crm/quotes', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get quote by ID
   */
  getById = async (id: string): Promise<QuoteDto> => {
    const response = await api.get(`/crm/quotes/${id}`, null);
    return response.data as QuoteDto;
  };

  /**
   * Get quotes by customer
   */
  getByCustomer = async (customerId: string): Promise<QuoteDto[]> => {
    const response = await api.get(`/crm/quotes/customer/${customerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get quotes by status
   */
  getByStatus = async (status: string): Promise<QuoteDto[]> => {
    const response = await api.get(`/crm/quotes/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get quotes by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<QuoteDto[]> => {
    const response = await api.get(`/crm/quotes/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new quote
   */
  create = async (quote: CreateQuoteDto): Promise<string> => {
    const response = await api.create('/crm/quotes', quote);
    return response.data.id || response.data.quoteId;
  };

  /**
   * Update quote
   */
  update = async (id: string, quote: UpdateQuoteDto): Promise<void> => {
    await api.update(`/crm/quotes/${id}`, { ...quote, id });
  };

  /**
   * Delete quote
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/quotes/${id}`);
  };

  /**
   * Send quote
   */
  sendQuote = async (id: string): Promise<void> => {
    await api.create(`/crm/quotes/${id}/send`, {});
  };

  /**
   * Convert quote to sales order
   */
  convertToSalesOrder = async (id: string): Promise<void> => {
    await api.create(`/crm/quotes/${id}/convert`, {});
  };
}

const quoteService = new QuoteService();
export default quoteService;

