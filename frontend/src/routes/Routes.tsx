import React from "react";
import { Navigate, Route, RouteObject, RouteProps, Routes } from "react-router-dom";

// All layouts containers
import DefaultLayout from "../layouts/Default";
import VerticalLayout from "../layouts/Vertical";

import { authProtectedFlattenRoutes, publicProtectedFlattenRoutes } from ".";
import { APICore } from "../helpers/api/apiCore";

const api = new APICore();

function renderProtectedElement(
  route: RouteObject,
  layoutProps: RouteProps
) {
  if (!api.isUserAuthenticated()) {
    api.setLoggedInUser(null);
    return (
      <Navigate
        to={{
          pathname: "/auth/login",
          search: "next=" + route.path,
        }}
        replace
      />
    );
  }

  const RouteLayout = route.path === "/modules" ? DefaultLayout : VerticalLayout;
  return (
    <RouteLayout {...layoutProps}>
      {route.element}
    </RouteLayout>
  );
}

const AllRoutes = (props: RouteProps) => {
  return (
    <React.Fragment>
      <Routes>
        <Route>
          {(publicProtectedFlattenRoutes || []).map((route: RouteObject, idx: number) => (
            <Route
              path={route.path}
              element={
                <DefaultLayout {...props}>
                  {route.element}
                </DefaultLayout>
              }
              key={idx}
            />
          ))}
          ;
        </Route>

        <Route>
          {(authProtectedFlattenRoutes || []).map((route: RouteObject, idx: number) => (
            <Route
              path={route.path}
              element={renderProtectedElement(route, props)}
              key={idx}
            />
          ))}
          ;
        </Route>
      </Routes>
    </React.Fragment>
  );
};

export default AllRoutes;
