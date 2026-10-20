import { APICore } from '../../../../../helpers/api/apiCore';
import type { JournalDto, CreateJournalDto, UpdateJournalDto, JournalQuery } from '../types';

const api = new APICore();

class JournalService {
  /**
   * Get all journals
   */
  getAll = async (query?: JournalQuery): Promise<JournalDto[]> => {
    const params: any = {};
    if (query?.journalType) params.journalType = query.journalType;
    if (query?.status) params.status = query.status;
    if (query?.fromDate) params.fromDate = query.fromDate;
    if (query?.toDate) params.toDate = query.toDate;
    if (query?.pageNumber) params.pageNumber = query.pageNumber;
    if (query?.pageSize) params.pageSize = query.pageSize;

    const queryString = new URLSearchParams(params).toString();
    const url = queryString ? `/finance/journals?${queryString}` : '/finance/journals';
    const response = await api.get(url, null);
    const result = response.data.Journals || response.data;
    return Array.isArray(result) ? result : [result];
  };

  /**
   * Get journal by ID
   */
  getById = async (id: string): Promise<JournalDto> => {
    const response = await api.get(`/finance/journals/${id}`, null);
    return response.data as JournalDto;
  };

  /**
   * Create new journal
   */
  create = async (journal: CreateJournalDto): Promise<string> => {
    const response = await api.create('/finance/journals', journal);
    return response.data.journalId || response.data.id;
  };

  /**
   * Update journal
   */
  update = async (id: string, journal: UpdateJournalDto): Promise<void> => {
    await api.update(`/finance/journals/${id}`, { ...journal, journalId: id });
  };

  /**
   * Delete journal
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/finance/journals/${id}`);
  };

  /**
   * Approve journal (Draft → Approved / Closed)
   */
  approve = async (id: string): Promise<void> => {
    await api.create(`/finance/journals/${id}/approve`, {});
  };

  /**
   * Reject approved journal back to Draft
   */
  reject = async (id: string, reason?: string): Promise<void> => {
    await api.create(`/finance/journals/${id}/reject`, { reason: reason || '', reversalReason: reason || '' });
  };

  /**
   * Post journal
   */
  post = async (id: string): Promise<void> => {
    await api.create(`/finance/journals/${id}/post`, {});
  };

  /**
   * Unpost journal (Posted → Draft)
   */
  unpost = async (id: string): Promise<void> => {
    await api.create(`/finance/journals/${id}/unpost`, {});
  };
}

const journalService = new JournalService();
export default journalService;

