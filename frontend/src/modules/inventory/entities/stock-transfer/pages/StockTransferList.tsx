import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { stockTransferService, type StockTransferDto } from "../services";
import { STOCK_TRANSFER_STATUS_OPTIONS } from "../types";

const StockTransferList: React.FC = () => {
  const [transfers, setTransfers] = useState<StockTransferDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadTransfers();
  }, [filterStatus]);

  const loadTransfers = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: StockTransferDto[];
      if (filterStatus) {
        data = await stockTransferService.getByStatus(filterStatus);
      } else {
        data = await stockTransferService.getAll();
      }
      setTransfers(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load stock transfers");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this stock transfer?")) return;

    try {
      await stockTransferService.delete(id);
      loadTransfers();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete stock transfer");
    }
  };

  const handleApprove = async (id: string) => {
    if (!window.confirm("Are you sure you want to approve this stock transfer?")) return;

    try {
      await stockTransferService.approve(id);
      loadTransfers();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to approve stock transfer");
    }
  };

  const handleComplete = async (id: string) => {
    if (!window.confirm("Are you sure you want to complete this stock transfer?")) return;

    try {
      await stockTransferService.complete(id);
      loadTransfers();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to complete stock transfer");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = STOCK_TRANSFER_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Completed":
        return "bg-success";
      case "Approved":
        return "bg-info";
      case "InTransit":
        return "bg-warning";
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

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("transferDate");
  const sortedTransfers = useMemo(
    () => sortRows(transfers),
    [transfers, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Stock Transfers" subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Stock Transfers</h4>
            <Link to="/inventory/stock-transfers/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Stock Transfer
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
              {STOCK_TRANSFER_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="transferNumber" label="Transfer Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="transferDate" label="Transfer Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="fromWarehouseName" label="From Warehouse" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="toWarehouseName" label="To Warehouse" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reference" label="Reference" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {transfers.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No stock transfers found
                      </td>
                    </tr>
                  ) : (
                    sortedTransfers.map((transfer) => (
                      <tr key={transfer.id}>
                        <td>{transfer.transferNumber}</td>
                        <td>{formatDate(transfer.transferDate)}</td>
                        <td>{transfer.fromWarehouseName || transfer.fromWarehouseId}</td>
                        <td>{transfer.toWarehouseName || transfer.toWarehouseId}</td>
                        <td>{transfer.reference || "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(transfer.status)}`}>
                            {getStatusLabel(transfer.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/stock-transfers/${transfer.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {transfer.status === "Draft" && (
                              <button
                                onClick={() => handleApprove(transfer.id || "")}
                                className="btn btn-sm btn-soft-success"
                                title="Approve Transfer"
                              >
                                <i className="mgc_check_line"></i>
                              </button>
                            )}
                            {transfer.status === "Approved" && (
                              <button
                                onClick={() => handleComplete(transfer.id || "")}
                                className="btn btn-sm btn-soft-info"
                                title="Complete Transfer"
                              >
                                <i className="mgc_check_circle_line"></i>
                              </button>
                            )}
                            {transfer.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(transfer.id || "")}
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

export default StockTransferList;

