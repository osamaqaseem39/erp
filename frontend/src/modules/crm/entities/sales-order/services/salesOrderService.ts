import { APICore } from '../../../../../helpers/api/apiCore';
import type { SalesOrderDto, CreateSalesOrderDto, UpdateSalesOrderDto } from '../types';

const api = new APICore();

class SalesOrderService {
  /**
   * Get all sales orders
   */
  getAll = async (): Promise<SalesOrderDto[]> => {
    const response = await api.get('/crm/sales-orders', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get sales order by ID
   */
  getById = async (id: string): Promise<SalesOrderDto> => {
    const response = await api.get(`/crm/sales-orders/${id}`, null);
    return response.data as SalesOrderDto;
  };

  /**
   * Get sales orders by customer
   */
  getByCustomer = async (customerId: string): Promise<SalesOrderDto[]> => {
    const response = await api.get(`/crm/sales-orders/customer/${customerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get sales orders by status
   */
  getByStatus = async (status: string): Promise<SalesOrderDto[]> => {
    const response = await api.get(`/crm/sales-orders/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get sales orders by date range
   */
  getByDateRange = async (startDate: string, endDate: string): Promise<SalesOrderDto[]> => {
    const response = await api.get(`/crm/sales-orders/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new sales order
   */
  create = async (salesOrder: CreateSalesOrderDto): Promise<string> => {
    const response = await api.create('/crm/sales-orders', salesOrder);
    return response.data.id || response.data.salesOrderId;
  };

  /**
   * Update sales order
   */
  update = async (id: string, salesOrder: UpdateSalesOrderDto): Promise<void> => {
    await api.update(`/crm/sales-orders/${id}`, { ...salesOrder, id });
  };

  /**
   * Delete sales order
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/crm/sales-orders/${id}`);
  };

  /**
   * Approve sales order
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/crm/sales-orders/${id}/approve`, {});
  };

  /**
   * Ship sales order
   */
  ship = async (id: string): Promise<void> => {
    await api.create(`/crm/sales-orders/${id}/ship`, {});
  };

  /**
   * Invoice sales order
   */
  invoice = async (id: string): Promise<void> => {
    await api.create(`/crm/sales-orders/${id}/invoice`, {});
  };
}

const salesOrderService = new SalesOrderService();
export default salesOrderService;

