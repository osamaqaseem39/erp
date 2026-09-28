import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../components";
// Use Core module service to avoid duplication
import { systemSettingService } from "../../../modules/core/entities/settings/services";
import type { SystemSettingDto } from "../../../modules/core/entities/settings/services";
import Swal from "sweetalert2";

const SystemSettings = () => {
  const [settings, setSettings] = useState<SystemSettingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>("");

  useEffect(() => {
    loadSettings();
  }, [selectedCategory]);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = await systemSettingService.getAll(selectedCategory || undefined);
      setSettings(data);
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to load system settings");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, key: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete setting ${key}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await systemSettingService.delete(id);
        Swal.fire("Deleted!", "Setting has been deleted.", "success");
        loadSettings();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete setting", "error");
      }
    }
  };

  const categories = Array.from(new Set(settings.map(s => s.category).filter(Boolean)));

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("key");
  const sortedSettings = useMemo(
    () => sortRows(settings),
    [settings, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="System Settings" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">System Settings</h4>
            <Link to="/erp/admin/system-settings/new" className="btn bg-primary text-white">
              <i className="mgc_add_line me-2"></i>Add Setting
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {categories.length > 0 && (
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2">
                Filter by Category
              </label>
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                className="form-select"
              >
                <option value="">All Categories</option>
                {categories.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat}
                  </option>
                ))}
              </select>
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
              <table className="w-full">
                <thead>
                <tr>
                <SortableTh columnKey="key" label="Key" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="value" label="Value" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="category" label="Category" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="dataType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {settings.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center p-8 text-gray-500">
                        No system settings found
                      </td>
                    </tr>
                  ) : (
                    settings.map((setting) => (
                      <tr key={setting.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3 font-medium">{setting.settingKey}</td>
                        <td className="p-3">
                          {setting.isEncrypted ? (
                            <span className="text-gray-400">••••••••</span>
                          ) : (
                            <span className="text-gray-600 dark:text-gray-300">{setting.settingValue}</span>
                          )}
                        </td>
                        <td className="p-3">
                          {setting.category ? (
                            <span className="badge bg-info">{setting.category}</span>
                          ) : (
                            <span className="text-gray-400">-</span>
                          )}
                        </td>
                        <td className="p-3">
                          <span className="badge bg-secondary">{setting.settingType}</span>
                        </td>
                        <td className="p-3 text-gray-500">{setting.description || '-'}</td>
                        <td className="p-3">
                          <div className="flex items-center gap-2">
                            {!setting.isReadOnly && (
                              <>
                                <Link
                                  to={`/erp/admin/system-settings/${setting.id}/edit`}
                                  className="btn btn-sm btn-soft-primary"
                                >
                                  <i className="mgc_edit_line"></i>
                                </Link>
                                <button
                                  onClick={async () => {
                                    const result = await Swal.fire({
                                      title: "Are you sure?",
                                      text: `Do you want to delete setting ${setting.settingKey}?`,
                                      icon: "warning",
                                      showCancelButton: true,
                                      confirmButtonColor: "#d33",
                                      cancelButtonColor: "#3085d6",
                                      confirmButtonText: "Yes, delete it!",
                                    });
                                    if (result.isConfirmed) {
                                      try {
                                        await systemSettingService.delete(setting.id);
                                        Swal.fire("Deleted!", "Setting has been deleted.", "success");
                                        loadSettings();
                                      } catch (err: any) {
                                        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete setting", "error");
                                      }
                                    }
                                  }}
                                  className="btn btn-sm btn-soft-danger"
                                >
                                  <i className="mgc_delete_line"></i>
                                </button>
                              </>
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

export default SystemSettings;

