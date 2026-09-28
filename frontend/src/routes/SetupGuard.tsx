import React from "react";
import { Navigate, useLocation } from "react-router-dom";
import SetupLoadingScreen from "../modules/core/components/SetupLoadingScreen";
import { useSetup } from "../modules/core/context/SetupContext";

const SETUP_PATH = "/setup";
const EXEMPT_PREFIXES = ["/maintenance", "/coming-soon", "/error-", "/access-denied"];

function isExemptPath(pathname: string): boolean {
  if (pathname === SETUP_PATH) return true;
  return EXEMPT_PREFIXES.some((prefix) => pathname.startsWith(prefix));
}

const SetupGuard: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isSetupComplete, isLoading } = useSetup();
  const { pathname } = useLocation();

  if (isLoading || isSetupComplete === null) {
    return <SetupLoadingScreen />;
  }

  if (!isSetupComplete && !isExemptPath(pathname)) {
    return <Navigate to={SETUP_PATH} replace state={{ from: pathname }} />;
  }

  if (isSetupComplete && pathname === SETUP_PATH) {
    return <Navigate to="/auth/login" replace />;
  }

  return <>{children}</>;
};

export default SetupGuard;
