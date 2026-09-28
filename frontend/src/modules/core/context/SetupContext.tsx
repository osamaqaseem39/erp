import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { SETUP_STATUS_KEY } from "../constants/setupKeys";
import { setupService } from "../services/setupService";

interface SetupContextValue {
  isSetupComplete: boolean | null;
  isLoading: boolean;
  refreshSetupStatus: () => Promise<void>;
  markSetupComplete: () => void;
}

const SetupContext = createContext<SetupContextValue | undefined>(undefined);

export const SetupProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isSetupComplete, setIsSetupComplete] = useState<boolean | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshSetupStatus = useCallback(async () => {
    setIsLoading(true);
    try {
      const exists = await setupService.checkSuperAdminExists();
      setIsSetupComplete(exists);
      if (exists) {
        sessionStorage.setItem(SETUP_STATUS_KEY, "true");
      } else {
        sessionStorage.removeItem(SETUP_STATUS_KEY);
      }
    } catch {
      setIsSetupComplete(false);
      sessionStorage.removeItem(SETUP_STATUS_KEY);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const markSetupComplete = useCallback(() => {
    setIsSetupComplete(true);
    sessionStorage.setItem(SETUP_STATUS_KEY, "true");
  }, []);

  useEffect(() => {
    // Only trust cached "complete" for a fast path; always verify with the API
    // so a stale "false" from a past failed check does not block redirect to login.
    if (sessionStorage.getItem(SETUP_STATUS_KEY) === "true") {
      setIsSetupComplete(true);
    }
    void refreshSetupStatus();
  }, [refreshSetupStatus]);

  const value = useMemo(
    () => ({
      isSetupComplete,
      isLoading,
      refreshSetupStatus,
      markSetupComplete,
    }),
    [isSetupComplete, isLoading, refreshSetupStatus, markSetupComplete]
  );

  return <SetupContext.Provider value={value}>{children}</SetupContext.Provider>;
};

export function useSetup(): SetupContextValue {
  const context = useContext(SetupContext);
  if (!context) {
    throw new Error("useSetup must be used within SetupProvider");
  }
  return context;
}
