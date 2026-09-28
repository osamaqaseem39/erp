import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { isCrmPath } from "./crmDynamicsTheme";

const CRM_THEME_ATTR = "data-crm-theme";
const CRM_THEME_VALUE = "dynamics";

/** Activates Dynamics 365 Sales styling on document root for CRM routes. */
export function useCrmDynamicsTheme(): boolean {
  const { pathname } = useLocation();
  const active = isCrmPath(pathname);

  useEffect(() => {
    const root = document.documentElement;
    const prevMenuColor = root.getAttribute("data-menu-color");

    if (active) {
      root.setAttribute(CRM_THEME_ATTR, CRM_THEME_VALUE);
      root.setAttribute("data-menu-color", "light");
    } else {
      root.removeAttribute(CRM_THEME_ATTR);
      if (prevMenuColor) {
        root.setAttribute("data-menu-color", prevMenuColor);
      }
    }

    return () => {
      root.removeAttribute(CRM_THEME_ATTR);
      if (prevMenuColor) {
        root.setAttribute("data-menu-color", prevMenuColor);
      }
    };
  }, [active]);

  return active;
}
