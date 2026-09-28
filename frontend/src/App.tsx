import AllRoutes from "./routes/Routes";
import SessionGuard from "./components/SessionGuard";
import { SetupProvider } from "./modules/core/context/SetupContext";
import SetupGuard from "./routes/SetupGuard";

// import { configureFakeBackend } from './helpers';

import "nouislider/distribute/nouislider.css";

import "./assets/scss/app.scss";
import "./assets/scss/icons.scss";

// configure fake backend - DISABLED: Using real API
// configureFakeBackend()

// Apollo Client is initialized in lib/apolloClient.ts and used directly in services
// No need for ApolloProvider wrapper when using client.query() and client.mutate()

const App = () => {

  return (
    <SetupProvider>
      <SetupGuard>
        <SessionGuard>
          <AllRoutes />
        </SessionGuard>
      </SetupGuard>
    </SetupProvider>
  );
}

export default App;
