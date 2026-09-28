import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { itemCategoryService, type ItemCategoryDto } from "../services";

const ItemCategoryList: React.FC = () => {
  const [categories, setCategories] = useState<ItemCategoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterIsActive, setFilterIsActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadCategories();
  }, [filterIsActive]);

  const loadCategories = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await itemCategoryService.getAll();
      let filtered = data;
      if (filterIsActive !== undefined) {
        filtered = data.filter((cat) => cat.isActive === filterIsActive);
      }
      setCategories(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load categories");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this category?")) return;

    try {
      await itemCategoryService.delete(id);
      loadCategories();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete category");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedCategories = useMemo(
    () => sortRows(categories),
    [categories, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Item Categories" subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Item Categories</h4>
            <Link to="/inventory/item-categories/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Category
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterIsActive === undefined ? "" : filterIsActive ? "true" : "false"}
              onChange={(e) =>
                setFilterIsActive(e.target.value === "" ? undefined : e.target.value === "true")
              }
            >
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {loading ? (
            <div className="text-center py-8">Loading...</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="code" label="Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="parentCategoryName" label="Parent Category" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {categories.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center py-8">
                        No categories found
                      </td>
                    </tr>
                  ) : (
                    sortedCategories.map((category) => (
                      <tr key={category.id}>
                        <td>{category.code}</td>
                        <td>{category.name}</td>
                        <td>{category.parentCategoryName || "Root"}</td>
                        <td>{category.description || "-"}</td>
                        <td>
                          {category.isActive ? (
                          <span className="badge badge-pill badge-soft-success">Active</span>
                          ) : (
                            <span className="badge bg-danger">Inactive</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/item-categories/${category.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(category.id || "")}
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

export default ItemCategoryList;

