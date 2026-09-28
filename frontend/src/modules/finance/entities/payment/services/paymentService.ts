import { APICore } from '../../../../../helpers/api/apiCore';
import type { PaymentDto, CreatePaymentDto, UpdatePaymentDto, PaymentQuery } from '../types';

const api = new APICore();

class PaymentService {
  /**
   * Get all payments
   */
  getAll = async (query?: PaymentQuery): Promise<PaymentDto[]> => {
    const params: any = {};
    if (query?.status) params.status = query.status;
    if (query?.paymentMethod) params.paymentMethod = query.paymentMethod;
    if (query?.fromDate) params.fromDate = query.fromDate;
    if (query?.toDate) params.toDate = query.toDate;
    if (query?.customerId) params.customerId = query.customerId;
    if (query?.invoiceId) params.invoiceId = query.invoiceId;

    const queryString = new URLSearchParams(params).toString();
    const url = queryString ? `/finance/payments?${queryString}` : '/finance/payments';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get payment by ID
   */
  getById = async (id: string): Promise<PaymentDto> => {
    const response = await api.get(`/finance/payments/${id}`, null);
    return response.data as PaymentDto;
  };

  /**
   * Get payments by invoice
   */
  getByInvoice = async (invoiceId: string): Promise<PaymentDto[]> => {
    const response = await api.get(`/finance/payments?invoiceId=${invoiceId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get payments by date range
   */
  getByDateRange = async (fromDate: string, toDate: string): Promise<PaymentDto[]> => {
    const response = await api.get(`/finance/payments?fromDate=${fromDate}&toDate=${toDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new payment
   */
  create = async (payment: CreatePaymentDto): Promise<string> => {
    const response = await api.create('/finance/payments', payment);
    return response.data.paymentId || response.data.id;
  };

  /**
   * Update payment
   */
  update = async (id: string, payment: UpdatePaymentDto): Promise<void> => {
    await api.update(`/finance/payments/${id}`, { ...payment, paymentId: id });
  };

  /**
   * Delete payment
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/payments/${id}`);
  };
}

const paymentService = new PaymentService();
export default paymentService;

