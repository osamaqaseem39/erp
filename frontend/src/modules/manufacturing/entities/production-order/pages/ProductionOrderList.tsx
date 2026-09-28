import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { productionOrderService, type ProductionOrderDto } from "../services";
import { PRODUCTION_ORDER_STATUS_OPTIONS, PRODUCTION_ORDER_PRIORITY_OPTIONS } from "../types";

const ProductionOrderList: React.FC = () => {
  const [productionOrders, setProductionOrders] = useState<ProductionOrderDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadProductionOrders();
  }, [filterStatus]);

  const loadProductionOrders = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: ProductionOrderDto[];
      if (filterStatus) {
        data = await productionOrderService.getAll(filterStatus);
      } else {
        data = await productionOrderService.getAll();
      }
      setProductionOrders(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load production orders");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this production order?")) return;

    try {
      await productionOrderService.delete(id);
      loadProductionOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete production order");
    }
  };

  const handleStart = async (id: string) => {
    if (!window.confirm("Are you sure you want to start this production order?")) return;

    try {
      await productionOrderService.start(id);
      alert("Production order started successfully!");
      loadProductionOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to start production order");
    }
  };

  const handleComplete = async (id: string) => {
    if (!window.confirm("Are you sure you want to complete this production order?")) return;

    try {
      await productionOrderService.complete(id);
      alert("Production order completed successfully!");
      loadProductionOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to complete production order");
    }
  };

  const handleCancel = async (id: string) => {
    const reason = window.prompt("Please enter cancellation reason:");
    if (!reason) return;

    try {
      await productionOrderService.cancel(id, reason);
      alert("Production order cancelled successfully!");
      loadProductionOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to cancel production order");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = PRODUCTION_ORDER_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "badge-soft-success";
      case "InProgress":
        return "badge-soft-info";
      case "Started":
        return "badge-soft-info";
      case "Planned":
        return "badge-soft-secondary";
      case "Cancelled":
        return "badge-soft-danger";
      case "OnHold":
        return "badge-soft-warning";
      default:
        return "badge-soft-secondary";
    }
  };

  const getPriorityBadgeClass = (priority?: string) => {
    switch (priority) {
      case "Urgent":
        return "badge-soft-danger";
      case "High":
        return "badge-soft-warning";
      case "Medium":
        return "badge-soft-info";
      case "Low":
        return "badge-soft-secondary";
      default:
        return "badge-soft-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const getProgress = (order: ProductionOrderDto): number => {
    if (order.quantity === 0) return 0;
    return Math.min(100, (order.completedQuantity / order.quantity) * 100);
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("plannedStartDate");
  const sortedProductionOrders = useMemo(
    () => sortRows(productionOrders),
    [productionOrders, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Production Orders" subName="Manufacturing" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Production Orders</h4>
            <Link to="/manufacturing/production-orders/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Production Order
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {PRODUCTION_ORDER_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {loading ? (
            <div className="text-center py-8">Loading...</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="productionOrderNumber" label="Production Order #" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="orderNumber" label="Order #" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="itemName" label="Item" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="quantity" label="Quantity" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="completedQuantity" label="Completed" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="progress" label="Progress" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="plannedStartDate" label="Planned Start" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="plannedEndDate" label="Planned End" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="priority" label="Priority" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {productionOrders.length === 0 ? (
                    <tr>
                      <td colSpan={11} className="text-center py-8">
                        No production orders found
                      </td>
                    </tr>
                  ) : (
                    sortedProductionOrders.map((order) => (
                      <tr key={order.id}>
                        <td>{order.productionOrderNumber}</td>
                        <td>{order.orderNumber || "-"}</td>
                        <td>{order.itemName || "-"}</td>
                        <td>{order.quantity.toFixed(2)}</td>
                        <td>{order.completedQuantity.toFixed(2)}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="bg-primary h-2 rounded-full"
                                style={{ width: `${getProgress(order)}%` }}
                              ></div>
                            </div>
                            <span className="text-sm">{getProgress(order).toFixed(0)}%</span>
                          </div>
                        </td>
                        <td>{formatDate(order.plannedStartDate)}</td>
                        <td>{formatDate(order.plannedEndDate)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(order.status)}`}>
                            {getStatusLabel(order.status)}
                          </span>
                        </td>
                        <td>
                          <span className={`badge ${getPriorityBadgeClass(order.priority)}`}>
                            {order.priority || "Medium"}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/manufacturing/production-orders/${order.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {order.status === "Planned" && (
                              <button
                                onClick={() => handleStart(order.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Start"
                              >
                                <i className="mgc_play_line"></i>
                              </button>
                            )}
                            {(order.status === "Started" || order.status === "InProgress") && (
                              <button
                                onClick={() => handleComplete(order.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Complete"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {order.status !== "Completed" && order.status !== "Cancelled" && (
                              <button
                                onClick={() => handleCancel(order.id || "")}
                                className="btn btn-sm btn-soft-danger"
                                title="Cancel"
                              >
                                <i className="mgc_close_line"></i>
                              </button>
                            )}
                            {order.status === "Planned" && (
                              <button
                                onClick={() => handleDelete(order.id || "")}
                                className="btn btn-sm btn-soft-danger"
                              >
                                <i className="mgc_delete_line"></i>
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default ProductionOrderList;

