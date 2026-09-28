// ProductionOrder Entity Types
// Based on ERP.Shared.DTOs.Manufacturing.ProductionOrderDto

export interface ProductionOrderDto {
  id?: string;
  tenantId?: string;
  productionOrderNumber: string;
  orderNumber: string;
  itemId?: string;
  workCenterId?: string;
  quantity: number;
  startDate?: string;
  endDate?: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  status: string;
  priority: string;
  completedQuantity: number;
  notes?: string;
  itemName?: string;
  cancellationReason?: string;
  createdBy?: string;
  updatedBy?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateProductionOrderDto {
  companyId?: string;
  productionOrderNumber?: string;
  orderNumber?: string;
  itemId?: string;
  workCenterId?: string;
  quantity?: number;
  startDate?: string;
  endDate?: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
  status?: string;
  priority?: string;
  notes?: string;
}

export interface UpdateProductionOrderDto {
  id: string;
  productionOrderNumber?: string;
  orderNumber?: string;
  itemId?: string;
  workCenterId?: string;
  quantity?: number;
  startDate?: string;
  endDate?: string;
  plannedStartDate?: string;
  plannedEndDate?: string;
  status?: string;
  priority?: string;
  notes?: string;
}

// Production Order Status Options
export const PRODUCTION_ORDER_STATUS_OPTIONS = [
  { value: "Planned", label: "Planned" },
  { value: "Started", label: "Started" },
  { value: "InProgress", label: "In Progress" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
  { value: "OnHold", label: "On Hold" },
];

// Production Order Priority Options
export const PRODUCTION_ORDER_PRIORITY_OPTIONS = [
  { value: "Low", label: "Low" },
  { value: "Medium", label: "Medium" },
  { value: "High", label: "High" },
  { value: "Urgent", label: "Urgent" },
];

