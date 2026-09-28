import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { APICore, forceLogout } from "../helpers/api/apiCore";

const PUBLIC_PREFIXES = ["/auth/", "/setup", "/maintenance", "/coming-soon", "/error-", "/access-denied", "/dev/"];

function isPublicPath(pathname: string): boolean {
  return PUBLIC_PREFIXES.some((prefix) => pathname.startsWith(prefix));
}

/**
 * Clears invalid sessions when the app loads or the tab regains focus.
 */
const SessionGuard: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { pathname } = useLocation();

  useEffect(() => {
    if (isPublicPath(pathname)) {
      return;
    }

    const api = new APICore();
    const stored = api.getLoggedInUser();

    if (stored?.token && !api.isUserAuthenticated()) {
      forceLogout("session_expired");
    }
  }, [pathname]);

  useEffect(() => {
    const onFocus = () => {
      if (isPublicPath(window.location.pathname)) {
        return;
      }
      const api = new APICore();
      const stored = api.getLoggedInUser();
      if (stored?.token && !api.isUserAuthenticated()) {
        forceLogout("session_expired");
      }
    };

    window.addEventListener("focus", onFocus);
    return () => window.removeEventListener("focus", onFocus);
  }, []);

  return <>{children}</>;
};

export default SessionGuard;
