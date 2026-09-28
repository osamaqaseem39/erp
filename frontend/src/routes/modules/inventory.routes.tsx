import React from "react";
import type { RoutesProps } from "../types";

const ItemCategoryForm = React.lazy(() => import("../../modules/inventory/entities/item-category/pages/ItemCategoryForm"));
const ItemCategoryList = React.lazy(() => import("../../modules/inventory/entities/item-category/pages/ItemCategoryList"));
const ItemForm = React.lazy(() => import("../../modules/inventory/entities/item/pages/ItemForm"));
const ItemList = React.lazy(() => import("../../modules/inventory/entities/item/pages/ItemList"));
const StockAdjustmentForm = React.lazy(() => import("../../modules/inventory/entities/stock-adjustment/pages/StockAdjustmentForm"));
const StockAdjustmentList = React.lazy(() => import("../../modules/inventory/entities/stock-adjustment/pages/StockAdjustmentList"));
const StockTransferForm = React.lazy(() => import("../../modules/inventory/entities/stock-transfer/pages/StockTransferForm"));
const StockTransferList = React.lazy(() => import("../../modules/inventory/entities/stock-transfer/pages/StockTransferList"));
const UnitOfMeasureForm = React.lazy(() => import("../../modules/inventory/entities/unit-of-measure/pages/UnitOfMeasureForm"));
const UnitOfMeasureList = React.lazy(() => import("../../modules/inventory/entities/unit-of-measure/pages/UnitOfMeasureList"));
const WarehouseForm = React.lazy(() => import("../../modules/inventory/entities/warehouse/pages/WarehouseForm"));
const WarehouseList = React.lazy(() => import("../../modules/inventory/entities/warehouse/pages/WarehouseList"));

export const inventoryRoutes: RoutesProps = {
  path: "/inventory",
  name: "Inventory",
  icon: "package",
  header: "Inventory Management",
  children: [
    // Item Categories
    {
      path: "/inventory/item-categories",
      name: "Item Categories",
      element: <ItemCategoryList />,
    },
    {
      path: "/inventory/item-categories/new",
      name: "Add Item Category",
      element: <ItemCategoryForm />,
    },
    {
      path: "/inventory/item-categories/:id/edit",
      name: "Edit Item Category",
      element: <ItemCategoryForm />,
    },
    // Warehouses
    {
      path: "/inventory/warehouses",
      name: "Warehouses",
      element: <WarehouseList />,
    },
    {
      path: "/inventory/warehouses/new",
      name: "Add Warehouse",
      element: <WarehouseForm />,
    },
    {
      path: "/inventory/warehouses/:id/edit",
      name: "Edit Warehouse",
      element: <WarehouseForm />,
    },
    // Units of Measure
    {
      path: "/inventory/units-of-measure",
      name: "Units of Measure",
      element: <UnitOfMeasureList />,
    },
    {
      path: "/inventory/units-of-measure/new",
      name: "Add Unit of Measure",
      element: <UnitOfMeasureForm />,
    },
    {
      path: "/inventory/units-of-measure/:id/edit",
      name: "Edit Unit of Measure",
      element: <UnitOfMeasureForm />,
    },
    // Items
    {
      path: "/inventory/items",
      name: "Items",
      element: <ItemList />,
    },
    {
      path: "/inventory/items/new",
      name: "Add Item",
      element: <ItemForm />,
    },
    {
      path: "/inventory/items/:id/edit",
      name: "Edit Item",
      element: <ItemForm />,
    },
    // Stock Transfers
    {
      path: "/inventory/stock-transfers",
      name: "Stock Transfers",
      element: <StockTransferList />,
    },
    {
      path: "/inventory/stock-transfers/new",
      name: "Add Stock Transfer",
      element: <StockTransferForm />,
    },
    {
      path: "/inventory/stock-transfers/:id/edit",
      name: "Edit Stock Transfer",
      element: <StockTransferForm />,
    },
    // Stock Adjustments
    {
      path: "/inventory/stock-adjustments",
      name: "Stock Adjustments",
      element: <StockAdjustmentList />,
    },
    {
      path: "/inventory/stock-adjustments/new",
      name: "Add Stock Adjustment",
      element: <StockAdjustmentForm />,
    },
    {
      path: "/inventory/stock-adjustments/:id/edit",
      name: "Edit Stock Adjustment",
      element: <StockAdjustmentForm />,
    },
  ],
};
