import React, { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { roleService, type PermissionDto, type AssignPermissionsDto } from "../services";
import Swal from "sweetalert2";

const PRIMARY_ACTIONS = ["view", "create", "update", "delete"] as const;

type ResourceRow = {
  resource: string;
  label: string;
  byAction: Record<string, PermissionDto>;
  extra: PermissionDto[];
};

function parsePermission(p: PermissionDto): { resource: string; action: string; module: string } {
  const action = (p.actionName || p.action || "").toLowerCase();
  const resource = p.moduleName || p.module || "";
  if (action && resource) {
    return { resource, action, module: resource.split(".")[0] || "other" };
  }
  const name = p.name || "";
  const lastDot = name.lastIndexOf(".");
  if (lastDot <= 0) return { resource: name || "other", action: "other", module: "other" };
  const res = name.slice(0, lastDot);
  return { resource: res, action: name.slice(lastDot + 1).toLowerCase(), module: res.split(".")[0] || "other" };
}

function humanize(code: string): string {
  const leaf = code.includes(".") ? code.slice(code.lastIndexOf(".") + 1) : code;
  return leaf.replace(/([a-z])([A-Z])/g, "$1 $2").replace(/[-_]/g, " ");
}

const AssignPermissionsForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [permissions, setPermissions] = useState<PermissionDto[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) void loadData();
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
      prev.includes(permissionId) ? prev.filter((pid) => pid !== permissionId) : [...prev, permissionId]
    );
  };

  const selectAll = () => setSelectedPermissions(permissions.map((p) => p.id));
  const clearAll = () => setSelectedPermissions([]);

  const groupedByModule = useMemo(() => {
    const byResource = new Map<string, ResourceRow>();
    for (const p of permissions) {
      const { resource, action, module } = parsePermission(p);
      let row = byResource.get(resource);
      if (!row) {
        row = { resource, label: humanize(resource), byAction: {}, extra: [] };
        byResource.set(resource, row);
      }
      if ((PRIMARY_ACTIONS as readonly string[]).includes(action)) {
        row.byAction[action] = p;
      } else {
        row.extra.push(p);
      }
      // stash module on row via resource prefix — regroup below
      (row as ResourceRow & { module?: string }).module = module;
    }

    const modules: Record<string, ResourceRow[]> = {};
    for (const row of byResource.values()) {
      const module = (row as ResourceRow & { module?: string }).module || row.resource.split(".")[0] || "other";
      if (!modules[module]) modules[module] = [];
      modules[module].push(row);
    }
    for (const rows of Object.values(modules)) {
      rows.sort((a, b) => a.resource.localeCompare(b.resource));
    }
    return Object.entries(modules).sort(([a], [b]) => a.localeCompare(b));
  }, [permissions]);

  const toggleResourceActions = (row: ResourceRow, on: boolean) => {
    const ids = [
      ...PRIMARY_ACTIONS.map((a) => row.byAction[a]?.id).filter(Boolean),
      ...row.extra.map((p) => p.id),
    ] as string[];
    setSelectedPermissions((prev) => {
      if (on) return Array.from(new Set([...prev, ...ids]));
      return prev.filter((id) => !ids.includes(id));
    });
  };

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
                <div className="space-y-8">
                  {groupedByModule.map(([moduleName, rows]) => (
                    <div key={moduleName}>
                      <h5 className="font-semibold text-gray-800 dark:text-gray-200 mb-3 uppercase tracking-wide text-sm">
                        {moduleName}
                      </h5>
                      <div className="table-responsive">
                        <table className="table table-sm table-hover mb-0">
                          <thead>
                            <tr>
                              <th className="w-1/3">Resource</th>
                              {PRIMARY_ACTIONS.map((a) => (
                                <th key={a} className="text-center capitalize">
                                  {a === "view" ? "View" : a === "create" ? "Create" : a === "update" ? "Update" : "Delete"}
                                </th>
                              ))}
                              <th>Other</th>
                              <th className="text-center">All</th>
                            </tr>
                          </thead>
                          <tbody>
                            {rows.map((row) => {
                              const allIds = [
                                ...PRIMARY_ACTIONS.map((a) => row.byAction[a]?.id).filter(Boolean),
                                ...row.extra.map((p) => p.id),
                              ] as string[];
                              const allOn = allIds.length > 0 && allIds.every((pid) => selectedPermissions.includes(pid));
                              return (
                                <tr key={row.resource}>
                                  <td>
                                    <div className="font-medium">{row.label}</div>
                                    <div className="text-xs text-gray-500">{row.resource}</div>
                                  </td>
                                  {PRIMARY_ACTIONS.map((action) => {
                                    const perm = row.byAction[action];
                                    if (!perm) {
                                      return (
                                        <td key={action} className="text-center text-gray-300">
                                          —
                                        </td>
                                      );
                                    }
                                    return (
                                      <td key={action} className="text-center">
                                        <input
                                          type="checkbox"
                                          className="form-checkbox rounded"
                                          checked={selectedPermissions.includes(perm.id)}
                                          onChange={() => togglePermission(perm.id)}
                                          title={perm.name}
                                          aria-label={`${row.resource}.${action}`}
                                        />
                                      </td>
                                    );
                                  })}
                                  <td>
                                    {row.extra.length === 0 ? (
                                      <span className="text-gray-300">—</span>
                                    ) : (
                                      <div className="flex flex-wrap gap-2">
                                        {row.extra.map((perm) => {
                                          const { action } = parsePermission(perm);
                                          return (
                                            <label key={perm.id} className="inline-flex items-center gap-1 text-sm">
                                              <input
                                                type="checkbox"
                                                className="form-checkbox rounded"
                                                checked={selectedPermissions.includes(perm.id)}
                                                onChange={() => togglePermission(perm.id)}
                                              />
                                              <span>{action}</span>
                                            </label>
                                          );
                                        })}
                                      </div>
                                    )}
                                  </td>
                                  <td className="text-center">
                                    <input
                                      type="checkbox"
                                      className="form-checkbox rounded"
                                      checked={allOn}
                                      onChange={(e) => toggleResourceActions(row, e.target.checked)}
                                      aria-label={`Toggle all for ${row.resource}`}
                                    />
                                  </td>
                                </tr>
                              );
                            })}
                          </tbody>
                        </table>
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
