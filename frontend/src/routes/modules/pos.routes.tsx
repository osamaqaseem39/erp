import React from "react";
import type { RoutesProps } from "../types";

const POSSessionForm = React.lazy(() => import("../../modules/pos/entities/pos-session/pages/POSSessionForm"));
const POSSessionList = React.lazy(() => import("../../modules/pos/entities/pos-session/pages/POSSessionList"));
const POSTransactionForm = React.lazy(() => import("../../modules/pos/entities/pos-transaction/pages/POSTransactionForm"));
const POSTransactionList = React.lazy(() => import("../../modules/pos/entities/pos-transaction/pages/POSTransactionList"));

export const posRoutes: RoutesProps = {
  path: "/pos",
  name: "POS",
  icon: "shopping-cart",
  header: "Point of Sale",
  children: [
    // POS Sessions
    {
      path: "/pos/sessions",
      name: "POS Sessions",
      element: <POSSessionList />,
    },
    {
      path: "/pos/sessions/new",
      name: "Add POS Session",
      element: <POSSessionForm />,
    },
    {
      path: "/pos/sessions/:id/edit",
      name: "Edit POS Session",
      element: <POSSessionForm />,
    },
    // POS Transactions
    {
      path: "/pos/transactions",
      name: "POS Transactions",
      element: <POSTransactionList />,
    },
    {
      path: "/pos/transactions/new",
      name: "Add POS Transaction",
      element: <POSTransactionForm />,
    },
    {
      path: "/pos/transactions/:id/edit",
      name: "Edit POS Transaction",
      element: <POSTransactionForm />,
    },
  ],
};
