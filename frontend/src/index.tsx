import React from "react";
import { RouterProvider } from "react-router-dom";
import { createRoot } from "react-dom/client";
import { Provider } from "react-redux";

import { router } from "./routes/router";
import { store } from "./redux/store.js";

const container = document.getElementById("konrix");
if (container) {
  const root = createRoot(container);
  root.render(
    <Provider store={store}>
      <React.Fragment>
        <RouterProvider router={router} future={{ v7_startTransition: true }} />
      </React.Fragment>
    </Provider>
  );
}
