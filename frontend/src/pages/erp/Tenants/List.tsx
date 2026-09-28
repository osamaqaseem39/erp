import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../components";
import { Link, useNavigate } from "react-router-dom";
import { PageBreadcrumb } from "../../../components";
// Use Core module service to avoid duplication
import { tenantService, type TenantDto } from "../../../modules/core/entities/tenant/services";

const TenantList = () => {
  const navigate = useNavigate();
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
      setError(err.response?.data?.message || "Failed to load tenants");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Are you sure you want to delete this tenant?")) {
      try {
        await tenantService.delete(id);
        loadTenants();
      } catch (err: any) {
        setError(err.response?.data?.message || "Failed to delete tenant");
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
      <PageBreadcrumb title="Tenants" subName="ERP" />
      <div className="card">
        <div className="card-header flex justify-between items-center">
          <h4 className="card-title">Tenants</h4>
          <Link to="/erp/tenants/new" className="btn btn-primary">
            <i className="mgc_add_line me-2"></i>Add Tenant
          </Link>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
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
            <div className="overflow-x-auto">
              <table className="table table-hover table-nowrap mb-0 w-full">
                <thead>
                <tr>
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="domain" label="Domain" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="industry" label="Industry" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="country" label="Country" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-end p-3 font-semibold" sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {tenants.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center p-8 text-gray-500">
                        No tenants found
                      </td>
                    </tr>
                  ) : (
                    sortedTenants.map((tenant) => (
                      <tr key={tenant.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3">{tenant.name}</td>
                        <td className="p-3 text-gray-500">{tenant.domain}</td>
                        <td className="p-3">{tenant.industry}</td>
                        <td className="p-3">{tenant.country}</td>
                        <td className="p-3">
                          <span className={`badge ${tenant.status === 'Active' ? 'bg-success' : 'bg-secondary'}`}>
                            {tenant.status}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => navigate(`/erp/tenants/${tenant.id}/edit`)}
                              className="btn btn-sm btn-soft-warning"
                            >
                              <i className="mgc_edit_line"></i>
                            </button>
                            <button
                              onClick={() => handleDelete(tenant.id)}
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

