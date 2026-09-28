import { APICore } from '../../../../../helpers/api/apiCore';
import type { ProjectDto, CreateProjectDto, UpdateProjectDto } from '../types';

const api = new APICore();

class ProjectService {
  /**
   * Get all projects
   */
  getAll = async (status?: string, managerId?: string, customerId?: string): Promise<ProjectDto[]> => {
    const params: any = {};
    if (status) params.status = status;
    if (managerId) params.managerId = managerId;
    if (customerId) params.customerId = customerId;

    const response = await api.get('/projects', params);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get project by ID
   */
  getById = async (id: string): Promise<ProjectDto> => {
    const response = await api.get(`/projects/${id}`, null);
    return response.data as ProjectDto;
  };

  /**
   * Get projects by status
   */
  getByStatus = async (status: string): Promise<ProjectDto[]> => {
    const response = await api.get(`/projects?status=${status}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get projects by manager
   */
  getByManager = async (managerId: string): Promise<ProjectDto[]> => {
    const response = await api.get(`/projects?managerId=${managerId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Get projects by client
   */
  getByClient = async (clientId: string): Promise<ProjectDto[]> => {
    const response = await api.get(`/projects?customerId=${clientId}`, null);
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  /**
   * Create new project
   */
  create = async (project: CreateProjectDto): Promise<string> => {
    const response = await api.create('/projects', project);
    return response.data.ProjectId || response.data.id || response.data;
  };

  /**
   * Update project
   */
  update = async (id: string, project: UpdateProjectDto): Promise<void> => {
    await api.update(`/projects/${id}`, { ...project, id });
  };

  /**
   * Delete project
   */
  delete = async (id: string): Promise<void> => {
    await api.delete(`/projects/${id}`);
  };

  /**
   * Start project
   */
  start = async (id: string): Promise<void> => {
    await api.create(`/projects/${id}/start`, {});
  };

  /**
   * Complete project
   */
  complete = async (id: string): Promise<void> => {
    await api.create(`/projects/${id}/complete`, {});
  };

  /**
   * Pause project
   */
  pause = async (id: string, reason?: string): Promise<void> => {
    await api.create(`/projects/${id}/pause`, { reason: reason || "" });
  };

  /**
   * Cancel project
   */
  cancel = async (id: string, reason?: string): Promise<void> => {
    await api.create(`/projects/${id}/cancel`, { reason: reason || "" });
  };
}

const projectService = new ProjectService();
export default projectService;

