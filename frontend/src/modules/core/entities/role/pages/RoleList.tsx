import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { roleService, type RoleDto } from "../services";
import Swal from "sweetalert2";

const RoleList: React.FC = () => {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadRoles();
  }, []);

  const loadRoles = async () => {
    try {
      setLoading(true);
      const data = await roleService.getAll();
      setRoles(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load roles");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete role ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await roleService.delete(id);
        Swal.fire("Deleted!", "Role has been deleted.", "success");
        loadRoles();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete role", "error");
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedRoles = useMemo(
    () => sortRows(roles),
    [roles, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Roles" subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Roles</h4>
            <Link to="/core/settings/roles/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Role
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
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {roles.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center py-8">
                        No roles found
                      </td>
                    </tr>
                  ) : (
                    sortedRoles.map((role) => (
                      <tr key={role.id}>
                        <td>{role.name}</td>
                        <td>{role.description || "-"}</td>
                        <td>
                          <span
                            className={`badge badge-pill ${
                              role.isActive ? "badge-soft-success" : "badge-soft-danger"
                            }`}
                          >
                            {role.isActive ? "Active" : "Inactive"}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/core/settings/roles/${role.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <Link
                              to={`/core/settings/roles/${role.id}/permissions`}
                              className="btn btn-sm btn-soft-info"
                            >
                              <i className="mgc_key_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(role.id, role.name)}
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

export default RoleList;

