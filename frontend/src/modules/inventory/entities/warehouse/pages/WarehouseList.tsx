import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { warehouseService, type WarehouseDto } from "../services";

const WarehouseList: React.FC = () => {
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterIsActive, setFilterIsActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadWarehouses();
  }, [filterIsActive]);

  const loadWarehouses = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await warehouseService.getAll(filterIsActive);
      setWarehouses(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load warehouses");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this warehouse?")) return;

    try {
      await warehouseService.delete(id);
      loadWarehouses();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete warehouse");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("warehouseName");
  const sortedWarehouses = useMemo(
    () => sortRows(warehouses),
    [warehouses, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Warehouses" subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Warehouses</h4>
            <Link to="/inventory/warehouses/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Warehouse
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterIsActive === undefined ? "" : filterIsActive ? "true" : "false"}
              onChange={(e) =>
                setFilterIsActive(e.target.value === "" ? undefined : e.target.value === "true")
              }
            >
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
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
                <SortableTh columnKey="warehouseCode" label="Warehouse Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="warehouseName" label="Warehouse Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="city" label="City" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="state" label="State" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="country" label="Country" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="contactPerson" label="Contact Person" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isDefault" label="Default" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {warehouses.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-8">
                        No warehouses found
                      </td>
                    </tr>
                  ) : (
                    sortedWarehouses.map((warehouse) => (
                      <tr key={warehouse.id}>
                        <td>{warehouse.warehouseCode}</td>
                        <td>{warehouse.warehouseName}</td>
                        <td>{warehouse.city}</td>
                        <td>{warehouse.state}</td>
                        <td>{warehouse.country}</td>
                        <td>{warehouse.contactPerson || "-"}</td>
                        <td>{warehouse.phoneNumber || "-"}</td>
                        <td>
                          {warehouse.isDefault ? (
                            <span className="badge bg-info">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>
                          {warehouse.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/warehouses/${warehouse.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(warehouse.id || "")}
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

export default WarehouseList;

