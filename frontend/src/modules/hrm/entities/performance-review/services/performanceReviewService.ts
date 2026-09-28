import { APICore } from '../../../../../helpers/api/apiCore';
import type { PerformanceReviewDto, CreatePerformanceReviewDto, UpdatePerformanceReviewDto } from '../types';

const api = new APICore();

class PerformanceReviewService {
  /**
   * Get all performance reviews
   */
  getAll = async (): Promise<PerformanceReviewDto[]> => {
    const response = await api.get('/hrm/performance', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get performance review by ID
   */
  getById = async (id: string): Promise<PerformanceReviewDto> => {
    const response = await api.get(`/hrm/performance/${id}`, null);
    return response.data as PerformanceReviewDto;
  };

  /**
   * Get performance reviews by employee
   */
  getByEmployee = async (employeeId: string): Promise<PerformanceReviewDto[]> => {
    const response = await api.get(`/hrm/performance/employee/${employeeId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get performance reviews by reviewer
   */
  getByReviewer = async (reviewerId: string): Promise<PerformanceReviewDto[]> => {
    const response = await api.get(`/hrm/performance/reviewer/${reviewerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get performance reviews by period
   */
  getByPeriod = async (startDate: string, endDate: string): Promise<PerformanceReviewDto[]> => {
    const response = await api.get(`/hrm/performance/period?startDate=${startDate}&endDate=${endDate}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new performance review
   */
  create = async (review: CreatePerformanceReviewDto): Promise<string> => {
    const response = await api.create('/hrm/performance', review);
    return response.data.id || response.data.reviewId;
  };

  /**
   * Update performance review
   */
  update = async (id: string, review: UpdatePerformanceReviewDto): Promise<void> => {
    await api.update(`/hrm/performance/${id}`, { ...review, id });
  };

  /**
   * Delete performance review
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/performance/${id}`);
  };
}

const performanceReviewService = new PerformanceReviewService();
export default performanceReviewService;

