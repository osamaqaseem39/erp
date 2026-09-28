import { APICore } from "../../../helpers/api/apiCore";

const api = new APICore();

/**
 * Returns the logged-in user's company id from session storage.
 */
export function useCompanyId(): string {
  const user = api.getLoggedInUser();
  return user?.companyId || user?.CompanyId || "";
}

export function getCompanyId(): string {
  const user = api.getLoggedInUser();
  return user?.companyId || user?.CompanyId || "";
}
