import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../components";
// Use Core module service to avoid duplication
import { roleService } from "../../../../modules/core/entities/role/services";
import Swal from "sweetalert2";

const AssignPermissions: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [permissions, setPermissions] = useState<any[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadPermissions();
      loadRolePermissions();
    }
  }, [id]);

  const loadPermissions = async () => {
    try {
      // This would need to be implemented - getting all available permissions
      // For now, we'll load permissions assigned to the role
      if (id) {
        const data = await roleService.getPermissions(id);
        setPermissions(data);
      }
    } catch (err: any) {
      console.error("Failed to load permissions", err);
    }
  };

  const loadRolePermissions = async () => {
    if (!id) return;
    try {
      const data = await roleService.getPermissions(id);
      setSelectedPermissions(data.map(p => p.id));
    } catch (err: any) {
      console.error("Failed to load role permissions", err);
    }
  };

  const handleSubmit = async () => {
    if (!id) return;

    setError(null);
    setLoading(true);
    try {
      await roleService.assignPermissions(id, {
        roleId: id,
        permissionIds: selectedPermissions,
      });
      Swal.fire("Success!", "Permissions have been assigned.", "success");
      navigate("/erp/admin/roles");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to assign permissions");
      Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to assign permissions", "error");
    } finally {
      setLoading(false);
    }
  };

  const togglePermission = (permissionId: string) => {
    setSelectedPermissions(prev =>
      prev.includes(permissionId)
        ? prev.filter(id => id !== permissionId)
        : [...prev, permissionId]
    );
  };

  return (
    <>
      <PageBreadcrumb title="Assign Permissions" subName="Admin" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/erp/admin/roles" className="btn btn-sm btn-soft-secondary">
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

          <div className="mb-4">
            <p className="text-gray-600 dark:text-gray-400">
              Select the permissions to assign to this role. Check the boxes to grant permissions.
            </p>
          </div>

          {permissions.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-gray-500">No permissions available</p>
            </div>
          ) : (
            <div className="space-y-2">
              {permissions.map((permission) => (
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
                    {permission.description && (
                      <div className="text-sm text-gray-500">{permission.description}</div>
                    )}
                  </label>
                </div>
              ))}
            </div>
          )}

          <div className="flex justify-end gap-2 mt-6">
            <Link to="/erp/admin/roles" className="btn btn-soft-secondary">
              Cancel
            </Link>
            <button
              onClick={handleSubmit}
              className="btn bg-primary text-white"
              disabled={loading}
            >
              {loading ? "Saving..." : "Save Permissions"}
            </button>
          </div>
        </div>
      </div>
    </>
  );
};

export default AssignPermissions;

