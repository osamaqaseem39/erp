import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { tenantService, type TenantDto } from "../services";
import Swal from "sweetalert2";

const TenantList: React.FC = () => {
  const [tenants, setTenants] = useState<TenantDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadTenants();
  }, []);

  const loadTenants = async () => {
    try {
      setLoading(true);
      const data = await tenantService.getAll();
      setTenants(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load tenants");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete tenant ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await tenantService.delete(id);
        Swal.fire("Deleted!", "Tenant has been deleted.", "success");
        loadTenants();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete tenant", "error");
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedTenants = useMemo(
    () => sortRows(tenants),
    [tenants, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Tenants" subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Tenants</h4>
            <Link to="/core/settings/tenants/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Tenant
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger mb-4 py-2 px-3" role="alert">
              {error}
            </div>
          )}

          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="domain" label="Domain" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="industry" label="Industry" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="country" label="Country" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {tenants.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center py-8">
                        No tenants found
                      </td>
                    </tr>
                  ) : (
                    sortedTenants.map((tenant) => (
                      <tr key={tenant.id}>
                        <td>{tenant.name}</td>
                        <td>{tenant.domain}</td>
                        <td>{tenant.industry || "-"}</td>
                        <td>{tenant.country || "-"}</td>
                        <td>
                          <span
                            className={`badge ${
                              tenant.status === "Active"
                                ? "badge-soft-success"
                                : "badge-soft-danger"
                            }`}
                          >
                            {tenant.status}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/core/settings/tenants/${tenant.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(tenant.id, tenant.name)}
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

export default TenantList;

