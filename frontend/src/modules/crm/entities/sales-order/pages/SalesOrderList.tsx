import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { salesOrderService, type SalesOrderDto } from "../services";
import { SALES_ORDER_STATUS_OPTIONS } from "../types";

const SalesOrderList: React.FC = () => {
  const [salesOrders, setSalesOrders] = useState<SalesOrderDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadSalesOrders();
  }, [filterStatus]);

  const loadSalesOrders = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: SalesOrderDto[];
      if (filterStatus) {
        data = await salesOrderService.getByStatus(filterStatus);
      } else {
        data = await salesOrderService.getAll();
      }
      setSalesOrders(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load sales orders");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this sales order?")) return;

    try {
      await salesOrderService.delete(id);
      loadSalesOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete sales order");
    }
  };

  const handleApprove = async (id: string) => {
    if (!window.confirm("Are you sure you want to approve this sales order?")) return;

    try {
      await salesOrderService.approve(id);
      alert("Sales order approved successfully!");
      loadSalesOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to approve sales order");
    }
  };

  const handleShip = async (id: string) => {
    if (!window.confirm("Are you sure you want to mark this sales order as shipped?")) return;

    try {
      await salesOrderService.ship(id);
      alert("Sales order marked as shipped!");
      loadSalesOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to ship sales order");
    }
  };

  const handleInvoice = async (id: string) => {
    if (!window.confirm("Are you sure you want to create an invoice for this sales order?")) return;

    try {
      await salesOrderService.invoice(id);
      alert("Invoice created successfully!");
      loadSalesOrders();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to create invoice");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = SALES_ORDER_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Invoiced":
        return "bg-info";
      case "Shipped":
        return "bg-primary";
      case "Approved":
        return "bg-warning";
      case "Pending":
        return "bg-secondary";
      case "Draft":
        return "bg-secondary";
      case "Cancelled":
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
  const sortedSalesOrders = useMemo(
    () => sortRows(salesOrders),
    [salesOrders, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Sales Orders" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Sales Orders</h4>
            <Link to="/crm/sales-orders/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Sales Order
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
              {SALES_ORDER_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="orderNumber" label="Order Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="quoteNumber" label="Quote" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="orderDate" label="Order Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="requiredDate" label="Required Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total Amount" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="assignedToUserName" label="Assigned To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {salesOrders.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No sales orders found
                      </td>
                    </tr>
                  ) : (
                    sortedSalesOrders.map((order) => (
                      <tr key={order.id}>
                        <td>{order.orderNumber || order.salesOrderNumber}</td>
                        <td>{order.customerName || "-"}</td>
                        <td>{order.quoteNumber || "-"}</td>
                        <td>{formatDate(order.orderDate)}</td>
                        <td>{formatDate(order.requiredDate)}</td>
                        <td>{order.totalAmount ? order.totalAmount.toFixed(2) : "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(order.status)}`}>
                            {getStatusLabel(order.status)}
                          </span>
                        </td>
                        <td>{order.assignedToUserName || "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/sales-orders/${order.id}/edit`}
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
                            {order.status === "Approved" && (
                              <button
                                onClick={() => handleShip(order.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Ship"
                              >
                                <i className="mgc_truck_line"></i>
                              </button>
                            )}
                            {order.status === "Shipped" && (
                              <button
                                onClick={() => handleInvoice(order.id || "")}
                                className="btn btn-sm btn-soft-warning"
                                title="Create Invoice"
                              >
                                <i className="mgc_file_list_line"></i>
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

export default SalesOrderList;

