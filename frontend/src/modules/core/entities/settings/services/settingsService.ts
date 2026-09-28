import { APICore } from '../../../../../helpers/api/apiCore';
import type { SettingsConfig, CompanySettings, SecuritySettings } from '../types';

const api = new APICore();

class SettingsService {
  /**
   * Get all settings
   */
  getAll = (): Promise<SettingsConfig[]> => {
    return api.get('/api/settings', {}).then((response) => response.data);
  };

  /**
   * Get setting by key
   */
  getByKey = (key: string): Promise<SettingsConfig> => {
    return api.get(`/api/settings/${key}`, {}).then((response) => response.data);
  };

  /**
   * Update setting
   */
  update = (key: string, value: string): Promise<SettingsConfig> => {
    return api.update(`/api/settings/${key}`, { value }).then((response) => response.data);
  };

  /**
   * Get company settings
   */
  getCompanySettings = (): Promise<CompanySettings> => {
    return api.get('/api/settings/company', {}).then((response) => response.data);
  };

  /**
   * Update company settings
   */
  updateCompanySettings = (data: CompanySettings): Promise<CompanySettings> => {
    return api.update('/api/settings/company', data).then((response) => response.data);
  };

  /**
   * Get security settings
   */
  getSecuritySettings = (): Promise<SecuritySettings> => {
    return api.get('/api/settings/security', {}).then((response) => response.data);
  };

  /**
   * Update security settings
   */
  updateSecuritySettings = (data: SecuritySettings): Promise<SecuritySettings> => {
    return api.update('/api/settings/security', data).then((response) => response.data);
  };
}

export const settingsService = new SettingsService();

