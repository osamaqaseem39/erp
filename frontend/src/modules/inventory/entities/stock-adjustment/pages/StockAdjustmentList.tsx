import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { stockAdjustmentService, type StockAdjustmentDto } from "../services";
import { STOCK_ADJUSTMENT_STATUS_OPTIONS, STOCK_ADJUSTMENT_TYPE_OPTIONS } from "../types";

const StockAdjustmentList: React.FC = () => {
  const [adjustments, setAdjustments] = useState<StockAdjustmentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");

  useEffect(() => {
    loadAdjustments();
  }, [filterStatus]);

  const loadAdjustments = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await stockAdjustmentService.getAll();
      let filtered = data;
      if (filterStatus) {
        filtered = data.filter((adj) => adj.status === filterStatus);
      }
      setAdjustments(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load stock adjustments");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this stock adjustment?")) return;

    try {
      await stockAdjustmentService.delete(id);
      loadAdjustments();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete stock adjustment");
    }
  };

  const getTypeLabel = (type?: string) => {
    const option = STOCK_ADJUSTMENT_TYPE_OPTIONS.find((opt) => opt.value === type);
    return option?.label || type || "N/A";
  };

  const getStatusLabel = (status?: string) => {
    const option = STOCK_ADJUSTMENT_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Posted":
        return "bg-success";
      case "Approved":
        return "bg-info";
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

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("adjustmentDate");
  const sortedAdjustments = useMemo(
    () => sortRows(adjustments),
    [adjustments, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Stock Adjustments" subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Stock Adjustments</h4>
            <Link to="/inventory/stock-adjustments/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Stock Adjustment
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
              {STOCK_ADJUSTMENT_STATUS_OPTIONS.map((option) => (
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
                <SortableTh columnKey="adjustmentNumber" label="Adjustment Number" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="adjustmentDate" label="Adjustment Date" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="warehouseName" label="Warehouse" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="adjustmentType" label="Adjustment Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="reference" label="Reference" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {adjustments.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="text-center py-8">
                        No stock adjustments found
                      </td>
                    </tr>
                  ) : (
                    sortedAdjustments.map((adjustment) => (
                      <tr key={adjustment.id}>
                        <td>{adjustment.adjustmentNumber}</td>
                        <td>{formatDate(adjustment.adjustmentDate)}</td>
                        <td>{adjustment.warehouseName || adjustment.warehouseId}</td>
                        <td>
                          <span className="badge bg-info">{getTypeLabel(adjustment.adjustmentType)}</span>
                        </td>
                        <td>{adjustment.reference || "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(adjustment.status)}`}>
                            {getStatusLabel(adjustment.status)}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/stock-adjustments/${adjustment.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {adjustment.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(adjustment.id || "")}
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

export default StockAdjustmentList;

