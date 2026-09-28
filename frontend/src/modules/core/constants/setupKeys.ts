export const SETUP_STATUS_KEY = "erp_setup_status";
export const SETUP_CREDENTIALS_KEY = "erp_setup_credentials";

export interface SetupCredentials {
  tenantId: string;
  email: string;
}

export function getSetupCredentials(): SetupCredentials | null {
  try {
    const raw = sessionStorage.getItem(SETUP_CREDENTIALS_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as SetupCredentials;
    if (parsed?.tenantId && parsed?.email) return parsed;
    return null;
  } catch {
    return null;
  }
}

export function saveSetupCredentials(credentials: SetupCredentials): void {
  sessionStorage.setItem(SETUP_CREDENTIALS_KEY, JSON.stringify(credentials));
}

export function clearSetupCredentials(): void {
  sessionStorage.removeItem(SETUP_CREDENTIALS_KEY);
}
