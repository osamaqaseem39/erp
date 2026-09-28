import React from "react";
import type { RoutesProps } from "../types";

const PurchaseOrderForm = React.lazy(() => import("../../modules/procurement/entities/purchase-order/pages/PurchaseOrderForm"));
const PurchaseOrderList = React.lazy(() => import("../../modules/procurement/entities/purchase-order/pages/PurchaseOrderList"));
const VendorForm = React.lazy(() => import("../../modules/procurement/entities/vendor/pages/VendorForm"));
const VendorList = React.lazy(() => import("../../modules/procurement/entities/vendor/pages/VendorList"));

export const procurementRoutes: RoutesProps = {
  path: "/procurement",
  name: "Procurement",
  icon: "truck",
  header: "Procurement Management",
  children: [
    // Vendors
    {
      path: "/procurement/vendors",
      name: "Vendors",
      element: <VendorList />,
    },
    {
      path: "/procurement/vendors/new",
      name: "Add Vendor",
      element: <VendorForm />,
    },
    {
      path: "/procurement/vendors/:id/edit",
      name: "Edit Vendor",
      element: <VendorForm />,
    },
    // Purchase Orders
    {
      path: "/procurement/purchase-orders",
      name: "Purchase Orders",
      element: <PurchaseOrderList />,
    },
    {
      path: "/procurement/purchase-orders/new",
      name: "Add Purchase Order",
      element: <PurchaseOrderForm />,
    },
    {
      path: "/procurement/purchase-orders/:id/edit",
      name: "Edit Purchase Order",
      element: <PurchaseOrderForm />,
    },
  ],
};
