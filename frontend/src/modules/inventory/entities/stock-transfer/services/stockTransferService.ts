import { APICore } from '../../../../../helpers/api/apiCore';
import type { StockTransferDto, CreateStockTransferDto, UpdateStockTransferDto } from '../types';

const api = new APICore();

class StockTransferService {
  /**
   * Get all stock transfers
   */
  getAll = async (): Promise<StockTransferDto[]> => {
    const response = await api.get('/inventory/stock-transfers', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get stock transfer by ID
   */
  getById = async (id: string): Promise<StockTransferDto> => {
    const response = await api.get(`/inventory/stock-transfers/${id}`, null);
    return response.data as StockTransferDto;
  };

  /**
   * Get stock transfers by from warehouse
   */
  getByFromWarehouse = async (warehouseId: string): Promise<StockTransferDto[]> => {
    const response = await api.get(`/inventory/stock-transfers/from-warehouse/${warehouseId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get stock transfers by to warehouse
   */
  getByToWarehouse = async (warehouseId: string): Promise<StockTransferDto[]> => {
    const response = await api.get(`/inventory/stock-transfers/to-warehouse/${warehouseId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get stock transfers by status
   */
  getByStatus = async (status: string): Promise<StockTransferDto[]> => {
    const response = await api.get(`/inventory/stock-transfers/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new stock transfer
   */
  create = async (transfer: CreateStockTransferDto): Promise<string> => {
    const response = await api.create('/inventory/stock-transfers', transfer);
    return response.data.id || response.data.transferId;
  };

  /**
   * Update stock transfer
   */
  update = async (id: string, transfer: UpdateStockTransferDto): Promise<void> => {
    await api.update(`/inventory/stock-transfers/${id}`, { ...transfer, id });
  };

  /**
   * Delete stock transfer
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/inventory/stock-transfers/${id}`);
  };

  /**
   * Approve stock transfer
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/inventory/stock-transfers/${id}/approve`, {});
  };

  /**
   * Complete stock transfer
   */
  complete = async (id: string): Promise<void> => {
    await api.create(`/inventory/stock-transfers/${id}/complete`, {});
  };
}

const stockTransferService = new StockTransferService();
export default stockTransferService;

