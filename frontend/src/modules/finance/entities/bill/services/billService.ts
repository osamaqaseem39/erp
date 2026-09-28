import { APICore } from '../../../../../helpers/api/apiCore';
import type { BillDto, CreateBillDto, UpdateBillDto, BillQuery } from '../types';

const api = new APICore();

class BillService {
  /**
   * Get all bills
   */
  getAll = async (query?: BillQuery): Promise<BillDto[]> => {
    const params: any = {};
    if (query?.vendorId) params.vendorId = query.vendorId;
    if (query?.status) params.status = query.status;
    if (query?.fromDate) params.fromDate = query.fromDate;
    if (query?.toDate) params.toDate = query.toDate;

    const queryString = new URLSearchParams(params).toString();
    const url = queryString ? `/finance/bills?${queryString}` : '/finance/bills';
    const response = await api.get(url, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get bill by ID
   */
  getById = async (id: string): Promise<BillDto> => {
    const response = await api.get(`/finance/bills/${id}`, null);
    return response.data as BillDto;
  };

  /**
   * Create new bill
   */
  create = async (bill: CreateBillDto): Promise<string> => {
    const response = await api.create('/finance/bills', bill);
    return response.data.billId || response.data.id;
  };

  /**
   * Update bill
   */
  update = async (id: string, bill: UpdateBillDto): Promise<void> => {
    await api.update(`/finance/bills/${id}`, { ...bill, billId: id });
  };

  /**
   * Delete bill
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/bills/${id}`);
  };

  /**
   * Approve bill
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/finance/bills/${id}/approve`, {});
  };
}

const billService = new BillService();
export default billService;

