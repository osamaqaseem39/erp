import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../components";
import { Link, useNavigate } from "react-router-dom";
import { PageBreadcrumb } from "../../../../components";
// Use Core module service to avoid duplication
import { roleService, type RoleDto } from "../../../../modules/core/entities/role/services";
import Swal from "sweetalert2";

const RoleList = () => {
  const navigate = useNavigate();
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
      setError(err.response?.data?.message || "Failed to load roles");
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
      <PageBreadcrumb title="Roles" subName="Admin" />
      <div className="card">
        <div className="card-header flex justify-between items-center">
          <h4 className="card-title">Roles</h4>
          <Link to="/erp/admin/roles/new" className="btn btn-primary">
            <i className="mgc_add_line me-2"></i>Add Role
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
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-end p-3 font-semibold" sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {roles.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center p-8 text-gray-500">
                        No roles found
                      </td>
                    </tr>
                  ) : (
                    sortedRoles.map((role) => (
                      <tr key={role.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3 font-medium">{role.name}</td>
                        <td className="p-3 text-gray-500">{role.description || '-'}</td>
                        <td className="p-3">
                          <span className={`badge ${role.isActive ? 'bg-success' : 'bg-secondary'}`}>
                            {role.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => navigate(`/erp/admin/roles/${role.id}/edit`)}
                              className="btn btn-sm btn-soft-warning"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </button>
                            <Link
                              to={`/erp/admin/roles/${role.id}/permissions`}
                              className="btn btn-sm btn-soft-info"
                              title="Assign Permissions"
                            >
                              <i className="mgc_key_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(role.id, role.name)}
                              className="btn btn-sm btn-soft-danger"
                              title="Delete"
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

