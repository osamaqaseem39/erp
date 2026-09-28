import { APICore } from '../../../../../helpers/api/apiCore';
import type { PurchaseOrderDto, CreatePurchaseOrderDto, UpdatePurchaseOrderDto } from '../types';

const api = new APICore();

class PurchaseOrderService {
  /**
   * Get all purchase orders
   */
  getAll = async (vendorId?: string, status?: string, fromDate?: string, toDate?: string): Promise<PurchaseOrderDto[]> => {
    const params: any = {};
    if (vendorId) params.vendorId = vendorId;
    if (status) params.status = status;
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;

    const response = await api.get('/procurement/purchase-orders', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get purchase order by ID
   */
  getById = async (id: string): Promise<PurchaseOrderDto> => {
    const response = await api.get(`/procurement/purchase-orders/${id}`, null);
    return response.data as PurchaseOrderDto;
  };

  /**
   * Get purchase orders by vendor
   */
  getByVendor = async (vendorId: string): Promise<PurchaseOrderDto[]> => {
    const response = await api.get(`/procurement/purchase-orders?vendorId=${vendorId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get purchase orders by status
   */
  getByStatus = async (status: string): Promise<PurchaseOrderDto[]> => {
    const response = await api.get(`/procurement/purchase-orders?status=${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get purchase orders by date range
   */
  getByDateRange = async (fromDate: string, toDate: string): Promise<PurchaseOrderDto[]> => {
    const response = await api.get(`/procurement/purchase-orders?fromDate=${fromDate}&toDate=${toDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new purchase order
   */
  create = async (purchaseOrder: CreatePurchaseOrderDto): Promise<string> => {
    const response = await api.create('/procurement/purchase-orders', purchaseOrder);
    return response.data.id || response.data.purchaseOrderId || response.data;
  };

  /**
   * Update purchase order
   */
  update = async (id: string, purchaseOrder: UpdatePurchaseOrderDto): Promise<void> => {
    await api.update(`/procurement/purchase-orders/${id}`, { ...purchaseOrder, id });
  };

  /**
   * Delete purchase order
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/procurement/purchase-orders/${id}`);
  };

  /**
   * Approve purchase order
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/procurement/purchase-orders/${id}/approve`, {});
  };

  /**
   * Reject purchase order
   */
  reject = async (id: string): Promise<void> => {
    await api.create(`/procurement/purchase-orders/${id}/reject`, {});
  };
}

const purchaseOrderService = new PurchaseOrderService();
export default purchaseOrderService;

