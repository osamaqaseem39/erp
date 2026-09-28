import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { salesTerritoryService, type SalesTerritoryDto } from "../services";
import Swal from "sweetalert2";

const SalesTerritoryList: React.FC = () => {
  const [salesTerritories, setSalesTerritories] = useState<SalesTerritoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadSalesTerritories();
  }, []);

  const loadSalesTerritories = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await salesTerritoryService.getAll();
      setSalesTerritories(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load sales territories");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete sales territory ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await salesTerritoryService.delete(id);
        Swal.fire("Deleted!", "Sales territory has been deleted.", "success");
        loadSalesTerritories();
      } catch (err: any) {
        Swal.fire(
          "Error!",
          err.response?.data?.Error || err.response?.data?.message || "Failed to delete sales territory",
          "error"
        );
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("territoryName");
  const sortedSalesTerritories = useMemo(
    () => sortRows(salesTerritories),
    [salesTerritories, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Sales Territories" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Sales Territories</h4>
            <Link to="/crm/sales-territories/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Sales Territory
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
                <SortableTh columnKey="territoryName" label="Territory Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="managerId" label="Manager ID" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {salesTerritories.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center py-8">
                        No sales territories found
                      </td>
                    </tr>
                  ) : (
                    sortedSalesTerritories.map((territory) => (
                      <tr key={territory.id}>
                        <td>{territory.territoryName || "-"}</td>
                        <td>{territory.description || "-"}</td>
                        <td>{territory.managerId || "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/sales-territories/${territory.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() =>
                                handleDelete(territory.id || "", territory.territoryName || "")
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

export default SalesTerritoryList;
