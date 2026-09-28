import { APICore } from '../../../../../helpers/api/apiCore';
import type { InvoiceDto, CreateInvoiceDto, UpdateInvoiceDto, InvoiceQuery } from '../types';

const api = new APICore();

class InvoiceService {
  /**
   * Get all invoices
   */
  getAll = async (query?: InvoiceQuery): Promise<InvoiceDto[]> => {
    const params: any = {};
    if (query?.customerId) params.customerId = query.customerId;
    if (query?.status) params.status = query.status;
    if (query?.startDate) params.fromDate = query.startDate;
    if (query?.endDate) params.toDate = query.endDate;

    const queryString = new URLSearchParams(params).toString();
    const url = queryString ? `/finance/invoices?${queryString}` : '/finance/invoices';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get invoice by ID
   */
  getById = async (id: string): Promise<InvoiceDto> => {
    const response = await api.get(`/finance/invoices/${id}`, null);
    return response.data as InvoiceDto;
  };

  /**
   * Create new invoice
   */
  create = async (invoice: CreateInvoiceDto): Promise<string> => {
    const response = await api.create('/finance/invoices', invoice);
    return response.data.invoiceId || response.data.id;
  };

  /**
   * Update invoice
   */
  update = async (id: string, invoice: UpdateInvoiceDto): Promise<void> => {
    await api.update(`/finance/invoices/${id}`, { ...invoice, invoiceId: id });
  };

  /**
   * Delete invoice
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/invoices/${id}`);
  };
}

const invoiceService = new InvoiceService();
export default invoiceService;

