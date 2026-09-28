import React from "react";
import type { RoutesProps } from "../types";
import { PermissionDenied } from "../../components/erp/PermissionDenied";

const ComingSoon = React.lazy(() => import("../../pages/error/ComingSoon"));
const Error404 = React.lazy(() => import("../../pages/error/Error404"));
const Error500 = React.lazy(() => import("../../pages/error/Error500"));
const LockScreen = React.lazy(() => import("../../pages/auth/LockScreen"));
const Login = React.lazy(() => import("../../pages/auth/Login"));
const Maintenance = React.lazy(() => import("../../pages/error/Maintenance"));
const RecoverPassword = React.lazy(() => import("../../pages/auth/RecoverPassword"));
const Register = React.lazy(() => import("../../pages/auth/Register"));
const RootRedirect = React.lazy(() => import("../RootRedirect"));
const SuperAdminSetup = React.lazy(() => import("../../pages/setup/SuperAdminSetup"));

export const authRoutes: RoutesProps[] = [
  {
    path: "/auth/login",
    name: "Login",
    element: <Login />,
  },
  {
    path: "/auth/register",
    name: "Register",
    element: <Register />,
  },
  {
    path: "/auth/recover-password",
    name: "Recover Password",
    element: <RecoverPassword />,
  },
  {
    path: "/auth/lock-screen",
    name: "Lock Screen",
    element: <LockScreen />,
  },
];

export const otherPublicRoutes: RoutesProps[] = [
  {
    path: "/",
    name: "Root",
    element: <RootRedirect />,
  },
  {
    path: "/setup",
    name: "Super Admin Setup",
    element: <SuperAdminSetup />,
  },
  {
    path: "*",
    name: "Error - 404",
    element: <Error404 />,
  },
  {
    path: "/access-denied",
    name: "Access denied",
    element: <PermissionDenied />,
  },
  {
    path: "/maintenance",
    name: "Maintenance",
    element: <Maintenance />,
  },
  {
    path: "/coming-soon",
    name: "Coming Soon",
    element: <ComingSoon />,
  },
  {
    path: "/error-404",
    name: "Error - 404",
    element: <Error404 />,
  },
  {
    path: "/error-500",
    name: "Error - 500",
    element: <Error500 />,
  },
];
