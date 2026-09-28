import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { returnOrderService, type ReturnOrderDto } from "../services";
import { RETURN_ORDER_STATUS_OPTIONS } from "../types";
import Swal from "sweetalert2";

const ReturnOrderList: React.FC = () => {
  const [returnOrders, setReturnOrders] = useState<ReturnOrderDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadReturnOrders();
  }, []);

  const loadReturnOrders = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await returnOrderService.getAll();
      setReturnOrders(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load return orders");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, returnNumber: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete return order ${returnNumber}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await returnOrderService.delete(id);
        Swal.fire("Deleted!", "Return order has been deleted.", "success");
        loadReturnOrders();
      } catch (err: any) {
        Swal.fire(
          "Error!",
          err.response?.data?.Error || err.response?.data?.message || "Failed to delete return order",
          "error"
        );
      }
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = RETURN_ORDER_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Approved":
        return "bg-info";
      case "Processing":
        return "bg-primary";
      case "Cancelled":
        return "bg-danger";
      case "Pending":
      default:
        return "bg-warning";
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("returnDate");
  const sortedReturnOrders = useMemo(
    () => sortRows(returnOrders),
    [returnOrders, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Return Orders" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Return Orders</h4>
            <Link to="/crm/return-orders/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Return Order
            </Link>
          </div>
        </div>
        <div className="card-body">
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
                <SortableTh columnKey="returnNumber" label="Return #" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="returnDate" label="Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="customerName" label="Customer" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reason" label="Reason" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="totalAmount" label="Total" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {returnOrders.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No return orders found
                      </td>
                    </tr>
                  ) : (
                    sortedReturnOrders.map((order) => (
                      <tr key={order.id}>
                        <td>{order.returnNumber || "-"}</td>
                        <td>{formatDate(order.returnDate)}</td>
                        <td>{order.customerName || order.customerId || "-"}</td>
                        <td>{order.reason || "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(order.status)}`}>
                            {getStatusLabel(order.status)}
                          </span>
                        </td>
                        <td>{order.totalAmount != null ? order.totalAmount.toFixed(2) : "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/return-orders/${order.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() =>
                                handleDelete(order.id || "", order.returnNumber || "")
                              }
                              className="btn btn-sm btn-soft-danger"
                            >
                              <i className="mgc_delete_line"></i>
                            </button>
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

export default ReturnOrderList;
