import React from "react";
import { useCrmDynamicsTheme } from "./useCrmDynamicsTheme";
import "./crmDynamics.css";

/** Mount inside app layout; toggles global CRM theme when route is under /crm */
const CrmThemeProvider: React.FC<{ children?: React.ReactNode }> = ({ children }) => {
  useCrmDynamicsTheme();
  return <>{children}</>;
};

export default CrmThemeProvider;
