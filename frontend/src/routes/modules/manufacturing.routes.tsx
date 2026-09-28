import React from "react";
import type { RoutesProps } from "../types";

const ProductionOrderForm = React.lazy(() => import("../../modules/manufacturing/entities/production-order/pages/ProductionOrderForm"));
const ProductionOrderList = React.lazy(() => import("../../modules/manufacturing/entities/production-order/pages/ProductionOrderList"));

export const manufacturingRoutes: RoutesProps = {
  path: "/manufacturing",
  name: "Manufacturing",
  icon: "factory",
  header: "Manufacturing Management",
  children: [
    // Production Orders
    {
      path: "/manufacturing/production-orders",
      name: "Production Orders",
      element: <ProductionOrderList />,
    },
    {
      path: "/manufacturing/production-orders/new",
      name: "Add Production Order",
      element: <ProductionOrderForm />,
    },
    {
      path: "/manufacturing/production-orders/:id/edit",
      name: "Edit Production Order",
      element: <ProductionOrderForm />,
    },
  ],
};
