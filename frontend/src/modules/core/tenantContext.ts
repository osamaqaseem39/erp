import { setFormatDefaults } from "../../lib/erp/format";
import { coreApi, type TenantContext } from "./services/coreApi";

const STORAGE_KEY = "erp.tenantContext";

function apply(context: TenantContext) {
  setFormatDefaults({ locale: context.locale, currency: context.currency, timeZone: context.timeZone });
}

function readCached(): TenantContext | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as TenantContext) : null;
  } catch {
    return null;
  }
}

/**
 * Applies the tenant's currency, locale and time zone to every formatter. Cached for the browser session,
 * so pages format correctly from the first render; refreshed after sign-in and when the company profile
 * or preferences change. Failure keeps the browser defaults (formatting still works, just less localized).
 */
export async function loadTenantContext(): Promise<void> {
  const cached = readCached();
  if (cached) {
    apply(cached);
    return;
  }
  await refreshTenantContext();
}

export async function refreshTenantContext(): Promise<void> {
  try {
    const context = await coreApi.context();
    apply(context);
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(context));
    } catch {
      // Storage unavailable: formatting still applies for this page view.
    }
  } catch {
    // Keep defaults; the next page load retries.
  }
}

export function clearTenantContext() {
  try {
    sessionStorage.removeItem(STORAGE_KEY);
  } catch {
    // ignore
  }
}
