// Settings module types
export interface SettingsConfig {
  id: string;
  key: string;
  value: string;
  category: string;
  description?: string;
}

export interface CompanySettings {
  name: string;
  email: string;
  phone: string;
  address: string;
  logo?: string;
  website?: string;
}

export interface SecuritySettings {
  passwordPolicy: {
    minLength: number;
    requireUppercase: boolean;
    requireLowercase: boolean;
    requireNumbers: boolean;
    requireSpecialChars: boolean;
  };
  sessionTimeout: number;
  twoFactorEnabled: boolean;
}

