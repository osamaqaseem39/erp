import { APICore } from '../../../../../helpers/api/apiCore';
import type { TrainingDto, CreateTrainingDto, UpdateTrainingDto } from '../types';

const api = new APICore();

class TrainingService {
  /**
   * Get all trainings
   */
  getAll = async (): Promise<TrainingDto[]> => {
    const response = await api.get('/hrm/training', null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get training by ID
   */
  getById = async (id: string): Promise<TrainingDto> => {
    const response = await api.get(`/hrm/training/${id}`, null);
    return response.data as TrainingDto;
  };

  /**
   * Get trainings by employee
   */
  getByEmployee = async (employeeId: string): Promise<TrainingDto[]> => {
    const response = await api.get(`/hrm/training/employee/${employeeId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get trainings by status
   */
  getByStatus = async (status: string): Promise<TrainingDto[]> => {
    const response = await api.get(`/hrm/training/status/${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new training
   */
  create = async (training: CreateTrainingDto): Promise<string> => {
    const response = await api.create('/hrm/training', training);
    return response.data.id || response.data.trainingId;
  };

  /**
   * Update training
   */
  update = async (id: string, training: UpdateTrainingDto): Promise<void> => {
    await api.update(`/hrm/training/${id}`, { ...training, id });
  };

  /**
   * Delete training
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/hrm/training/${id}`);
  };

  /**
   * Enroll employee in training
   */
  enrollEmployee = async (trainingId: string, employeeId: string): Promise<void> => {
    await api.create(`/hrm/training/${trainingId}/enroll`, { employeeId });
  };

  /**
   * Complete training for employee
   */
  completeTraining = async (trainingId: string, employeeId: string, score: number): Promise<void> => {
    await api.create(`/hrm/training/${trainingId}/complete`, { employeeId, score });
  };
}

const trainingService = new TrainingService();
export default trainingService;

