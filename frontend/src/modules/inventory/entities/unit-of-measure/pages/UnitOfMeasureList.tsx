import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { unitOfMeasureService, type UnitOfMeasureDto } from "../services";

const UnitOfMeasureList: React.FC = () => {
  const [units, setUnits] = useState<UnitOfMeasureDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterIsActive, setFilterIsActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadUnits();
  }, [filterIsActive]);

  const loadUnits = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await unitOfMeasureService.getAll(filterIsActive);
      setUnits(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load units");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this unit of measure?")) return;

    try {
      await unitOfMeasureService.delete(id);
      loadUnits();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete unit");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedUnits = useMemo(
    () => sortRows(units),
    [units, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Units of Measure" subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Units of Measure</h4>
            <Link to="/inventory/units-of-measure/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Unit of Measure
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
                <SortableTh columnKey="code" label="Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="symbol" label="Symbol" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="conversionFactor" label="Conversion Factor" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="baseUnitName" label="Base Unit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isBaseUnit" label="Is Base Unit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {units.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8">
                        No units of measure found
                      </td>
                    </tr>
                  ) : (
                    sortedUnits.map((unit) => (
                      <tr key={unit.id}>
                        <td>{unit.code}</td>
                        <td>{unit.name}</td>
                        <td>{unit.symbol}</td>
                        <td>{unit.conversionFactor}</td>
                        <td>{unit.baseUnitName || "-"}</td>
                        <td>
                          {unit.isBaseUnit ? (
                            <span className="badge bg-info">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>
                          {unit.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/units-of-measure/${unit.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(unit.id || "")}
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

export default UnitOfMeasureList;

