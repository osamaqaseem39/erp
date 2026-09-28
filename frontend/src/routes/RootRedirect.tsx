import { Navigate } from "react-router-dom";
import { APICore } from "../helpers/api/apiCore";
import SetupLoadingScreen from "../modules/core/components/SetupLoadingScreen";
import { useSetup } from "../modules/core/context/SetupContext";

const RootRedirect = () => {
  const { isSetupComplete, isLoading } = useSetup();
  const api = new APICore();

  if (isLoading || isSetupComplete === null) {
    return <SetupLoadingScreen />;
  }

  if (!isSetupComplete) {
    return <Navigate to="/setup" replace />;
  }

  if (api.isUserAuthenticated()) {
    return <Navigate to="/modules" replace />;
  }

  return <Navigate to="/auth/login" replace />;
};

export default RootRedirect;
