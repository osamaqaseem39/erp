import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { roleService, type PermissionDto, type AssignPermissionsDto } from "../services";
import Swal from "sweetalert2";

const AssignPermissionsForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      void loadData();
    }
  }, [id]);

  const loadData = async () => {
    if (!id) return;

    setLoadingData(true);
    setError(null);
    try {
      const [allPermissions, rolePermissions] = await Promise.all([
        roleService.getAllPermissions(),
        roleService.getPermissions(id),
      ]);
      setPermissions(allPermissions);
      setSelectedPermissions(rolePermissions.map((p) => p.id));
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Failed to load permissions";
      setError(message);
      console.error("Failed to load permissions", err);
    } finally {
      setLoadingData(false);
    }
  };

  const handleSubmit = async () => {
    if (!id) return;

    setError(null);
    setLoading(true);
    try {
      const dto: AssignPermissionsDto = {
        roleId: id,
        permissionIds: selectedPermissions,
      };
      await roleService.assignPermissions(id, dto);
      Swal.fire("Success!", "Permissions have been assigned.", "success");
      navigate("/core/settings/roles");
    } catch (err: unknown) {
      const apiError = err as { response?: { data?: { Error?: string; message?: string } }; message?: string };
      const errorMessage =
        apiError.response?.data?.Error ||
        apiError.response?.data?.message ||
        apiError.message ||
        "Failed to assign permissions";
      setError(errorMessage);
      Swal.fire("Error!", errorMessage, "error");
    } finally {
      setLoading(false);
    }
  };

  const togglePermission = (permissionId: string) => {
    setSelectedPermissions((prev) =>
      prev.includes(permissionId)
        ? prev.filter((pid) => pid !== permissionId)
        : [...prev, permissionId]
    );
  };

  const selectAll = () => setSelectedPermissions(permissions.map((p) => p.id));
  const clearAll = () => setSelectedPermissions([]);

  const groupedPermissions = permissions.reduce<Record<string, PermissionDto[]>>((groups, permission) => {
    const moduleName = permission.module || permission.moduleName || "Other";
    if (!groups[moduleName]) {
      groups[moduleName] = [];
    }
    groups[moduleName].push(permission);
    return groups;
  }, {});

  return (
    <>
      <PageBreadcrumb title="Assign Permissions" subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/core/settings/roles" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">Assign Permissions to Role</h4>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {loadingData ? (
            <div className="text-center py-8">
              <p className="text-gray-500">Loading permissions...</p>
            </div>
          ) : (
            <>
              <div className="flex flex-wrap items-center justify-between gap-2 mb-4">
                <p className="text-gray-600 dark:text-gray-400 mb-0">
                  {selectedPermissions.length} of {permissions.length} permissions selected
                </p>
                <div className="flex gap-2">
                  <button type="button" className="btn btn-sm btn-soft-primary" onClick={selectAll}>
                    Select all
                  </button>
                  <button type="button" className="btn btn-sm btn-soft-secondary" onClick={clearAll}>
                    Clear all
                  </button>
                </div>
              </div>

              {permissions.length === 0 ? (
                <div className="text-center py-8">
                  <p className="text-gray-500 mb-4">No permissions found for this tenant.</p>
                  <button type="button" className="btn btn-soft-primary" onClick={() => void loadData()}>
                    Retry
                  </button>
                </div>
              ) : (
                <div className="space-y-6">
                  {Object.entries(groupedPermissions).map(([moduleName, modulePermissions]) => (
                    <div key={moduleName}>
                      <h5 className="font-semibold text-gray-800 dark:text-gray-200 mb-2">{moduleName}</h5>
                      <div className="space-y-2">
                        {modulePermissions.map((permission) => (
                          <div key={permission.id} className="flex items-center p-3 border rounded">
                            <input
                              type="checkbox"
                              id={`perm-${permission.id}`}
                              checked={selectedPermissions.includes(permission.id)}
                              onChange={() => togglePermission(permission.id)}
                              className="form-checkbox rounded me-3"
                            />
                            <label htmlFor={`perm-${permission.id}`} className="flex-1 cursor-pointer">
                              <div className="font-medium">{permission.name}</div>
                              {(permission.description || permission.actionName) && (
                                <div className="text-sm text-gray-500">
                                  {permission.description || permission.actionName}
                                </div>
                              )}
                            </label>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex justify-end gap-2 mt-6">
                <Link to="/core/settings/roles" className="btn btn-soft-secondary">
                  Cancel
                </Link>
                <button
                  onClick={() => void handleSubmit()}
                  className="btn btn-primary"
                  disabled={loading || loadingData}
                >
                  {loading ? "Saving…" : "Save Permissions"}
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </>
  );
};

export default AssignPermissionsForm;
