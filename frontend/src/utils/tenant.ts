/**
 * Utility functions for tenant detection
 */

/**
 * Gets tenant ID from subdomain
 * Example: company1.erp.com -> company1
 */
export const getTenantFromSubdomain = (): string | null => {
  if (typeof window === 'undefined') return null;
  
  const hostname = window.location.hostname;
  const parts = hostname.split('.');
  
  // If we have subdomain (e.g., company1.erp.com)
  if (parts.length > 2) {
    return parts[0];
  }
  
  // If localhost with port (e.g., company1.localhost:5173)
  if (hostname.includes('localhost') && parts.length > 1) {
    return parts[0] !== 'localhost' ? parts[0] : null;
  }
  
  return null;
};

/**
 * Gets tenant ID from URL query parameter
 * Example: ?tenant=company1 or ?tenantId=xxx
 */
export const getTenantFromQuery = (): string | null => {
  if (typeof window === 'undefined') return null;
  
  const params = new URLSearchParams(window.location.search);
  return params.get('tenant') || params.get('tenantId');
};

/**
 * Gets tenant ID from localStorage (if previously set)
 */
export const getTenantFromStorage = (): string | null => {
  if (typeof window === 'undefined') return null;
  
  return localStorage.getItem('tenantId') || sessionStorage.getItem('tenantId');
};

/**
 * Gets tenant ID from multiple sources (subdomain, query, storage)
 */
export const getTenantId = (): string | null => {
  return getTenantFromQuery() || getTenantFromSubdomain() || getTenantFromStorage();
};

/**
 * Gets tenant domain from subdomain
 */
export const getTenantDomain = (): string | null => {
  return getTenantFromSubdomain();
};

