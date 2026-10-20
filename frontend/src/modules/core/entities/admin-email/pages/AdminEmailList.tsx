import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { PageBreadcrumb, SortableTh, useTableSort } from "../../../../../components";
import Swal from "sweetalert2";
import { adminEmailService, type AdminEmailDto } from "../services/adminEmailService";

const AdminEmailList: React.FC = () => {
  const [rows, setRows] = useState<AdminEmailDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      setLoading(true);
      setRows(await adminEmailService.getAll());
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load admin emails");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id: string, email: string) => {
    const result = await Swal.fire({
      title: "Deactivate this email?",
      text: email,
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Yes, deactivate",
    });
    if (!result.isConfirmed) return;
    try {
      await adminEmailService.delete(id);
      Swal.fire("Done", "Admin email deactivated.", "success");
      load();
    } catch (err: any) {
      Swal.fire("Error", err.response?.data?.Error || err.response?.data?.message || "Failed to deactivate", "error");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("email");
  const sorted = useMemo(() => sortRows(rows), [rows, sortRows]);

  return (
    <>
      <PageBreadcrumb title="Admin emails" subName="Core" />
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <h4 className="card-title">Admin emails</h4>
          <Link to="/core/settings/admin-emails/new" className="btn btn-primary">
            <i className="mgc_add_line me-2" />
            Add email
          </Link>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status" />
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                  <tr>
                    <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                    <SortableTh columnKey="type" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                    <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sorted.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center py-8">
                        No admin emails yet
                      </td>
                    </tr>
                  ) : (
                    sorted.map((row) => (
                      <tr key={row.id}>
                        <td>{row.email}</td>
                        <td>{row.type}</td>
                        <td>
                          <span className={`badge badge-pill ${row.isActive ? "badge-soft-success" : "badge-soft-danger"}`}>
                            {row.isActive ? "Active" : "Inactive"}
                          </span>
                        </td>
                        <td>
                          <div className="flex gap-2">
                            <Link to={`/core/settings/admin-emails/${row.id}/edit`} className="btn btn-sm btn-soft-primary">
                              <i className="mgc_edit_line" />
                            </Link>
                            {row.isActive && (
                              <button type="button" className="btn btn-sm btn-soft-danger" onClick={() => handleDelete(row.id, row.email)}>
                                <i className="mgc_delete_line" />
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

export default AdminEmailList;
