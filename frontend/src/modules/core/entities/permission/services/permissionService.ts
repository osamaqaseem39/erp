import { APICore } from "../../../../../helpers/api/apiCore";
import type { PermissionDto } from "../../role/services/roleService";

const api = new APICore();

class PermissionService {
  getAll = async (activeOnly = true): Promise<PermissionDto[]> => {
    const response = await api.get("/admin/permissions", { activeOnly });
    return Array.isArray(response.data) ? response.data : [response.data];
  };

  seedDefaults = async (): Promise<PermissionDto[]> => {
    const response = await api.create("/admin/permissions/seed", {});
    return Array.isArray(response.data) ? response.data : [response.data];
  };
}

export const permissionService = new PermissionService();
