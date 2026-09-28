/**
 * Returns the `next` query parameter only when it is a same-site path ("/crm/customers?page=2"),
 * so a crafted login link cannot send users to another site after they sign in.
 */
export function safeRedirectPath(search: string, fallback = "/modules"): string {
  const next = new URLSearchParams(search).get("next");
  if (!next || !next.startsWith("/") || next.startsWith("//") || next.startsWith("/\\")) return fallback;
  return next;
}
