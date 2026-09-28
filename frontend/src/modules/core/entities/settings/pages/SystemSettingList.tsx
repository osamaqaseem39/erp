import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { systemSettingService, type SystemSettingDto } from "../services";
import Swal from "sweetalert2";

const SystemSettingList: React.FC = () => {
  const [settings, setSettings] = useState<SystemSettingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [category, setCategory] = useState<string>("");

  useEffect(() => {
    loadSettings();
  }, [category]);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const data = category
        ? await systemSettingService.getByCategory(category)
        : await systemSettingService.getAll();
      setSettings(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load settings");
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

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("key");
  const sortedSettings = useMemo(
    () => sortRows(settings),
    [settings, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="System Settings" subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">System Settings</h4>
            <Link to="/core/settings/system-settings/new" className="btn btn-primary">
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

          <div className="mb-4">
            <input
              type="text"
              placeholder="Filter by category..."
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              className="form-input"
            />
          </div>

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
                <SortableTh columnKey="key" label="Key" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="value" label="Value" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="dataType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="category" label="Category" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {settings.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center py-8">
                        No settings found
                      </td>
                    </tr>
                  ) : (
                    sortedSettings.map((setting) => (
                      <tr key={setting.id}>
                        <td>{setting.settingKey}</td>
                        <td>{setting.isEncrypted ? "***" : setting.settingValue}</td>
                        <td>{setting.settingType}</td>
                        <td>{setting.category}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            {!setting.isReadOnly && (
                              <>
                                <Link
                                  to={`/core/settings/system-settings/${setting.id}/edit`}
                                  className="btn btn-sm btn-soft-primary"
                                >
                                  <i className="mgc_edit_line"></i>
                                </Link>
                                <button
                                  onClick={() => handleDelete(setting.id, setting.settingKey)}
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

export default SystemSettingList;

