import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { purchaseOrderService, type PurchaseOrderDto } from "../services";
import { PURCHASE_ORDER_STATUS_OPTIONS } from "../types";

const PurchaseOrderList: React.FC = () => {
  const [purchaseOrders, setPurchaseOrders] = useState<PurchaseOrderDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadPurchaseOrders();
  }, [filterStatus]);

  const loadPurchaseOrders = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: PurchaseOrderDto[];
      if (filterStatus) {
        data = await purchaseOrderService.getByStatus(filterStatus);
      } else {
        data = await purchaseOrderService.getAll();
      }
      setPurchaseOrders(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load purchase orders");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this purchase order?")) return;

    try {
      await purchaseOrderService.delete(id);
      loadPurchaseOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete purchase order");
    }
  };

  const handleApprove = async (id: string) => {
    if (!window.confirm("Are you sure you want to approve this purchase order?")) return;

    try {
      await purchaseOrderService.approve(id);
      alert("Purchase order approved successfully!");
      loadPurchaseOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to approve purchase order");
    }
  };

  const handleReject = async (id: string) => {
    if (!window.confirm("Are you sure you want to reject this purchase order?")) return;

    try {
      await purchaseOrderService.reject(id);
      alert("Purchase order rejected!");
      loadPurchaseOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to reject purchase order");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = PURCHASE_ORDER_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "PartiallyReceived":
        return "bg-info";
      case "Issued":
        return "bg-primary";
      case "Approved":
        return "bg-warning";
      case "Draft":
        return "bg-secondary";
      case "Cancelled":
        return "bg-danger";
      case "Rejected":
        return "bg-danger";
      default:
        return "bg-secondary";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("orderDate");
  const sortedPurchaseOrders = useMemo(
    () => sortRows(purchaseOrders),
    [purchaseOrders, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Purchase Orders" subName="Procurement" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Purchase Orders</h4>
            <Link to="/procurement/purchase-orders/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Purchase Order
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
              {PURCHASE_ORDER_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="poNumber" label="PO Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="vendorName" label="Vendor" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="orderDate" label="Order Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="expectedDeliveryDate" label="Expected Delivery" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {purchaseOrders.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No purchase orders found
                      </td>
                    </tr>
                  ) : (
                    sortedPurchaseOrders.map((order) => (
                      <tr key={order.id}>
                        <td>{order.purchaseOrderNumber}</td>
                        <td>{order.vendorName || order.vendorCode || "-"}</td>
                        <td>{formatDate(order.orderDate)}</td>
                        <td>{formatDate(order.expectedDeliveryDate)}</td>
                        <td>{order.totalAmount ? order.totalAmount.toFixed(2) : "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(order.status)}`}>
                            {getStatusLabel(order.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/procurement/purchase-orders/${order.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {order.status === "Draft" && (
                              <button
                                onClick={() => handleApprove(order.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Approve"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {order.status === "Draft" && (
                              <button
                                onClick={() => handleReject(order.id || "")}
                                className="btn btn-sm btn-soft-danger"
                                title="Reject"
                              >
                                <i className="mgc_close_line"></i>
                              </button>
                            )}
                            {order.status === "Draft" && (
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

export default PurchaseOrderList;

