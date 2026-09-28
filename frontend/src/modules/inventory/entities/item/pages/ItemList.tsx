import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { itemService } from "../services";
import type { ItemDto } from "../types";
import { itemCategoryService } from "../../item-category/services";
import { ITEM_TYPE_OPTIONS } from "../types";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";

const ItemList: React.FC = () => {
  const [items, setItems] = useState<ItemDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterCategoryId, setFilterCategoryId] = useState<string>("");
  const [categories, setCategories] = useState<any[]>([]);

  useEffect(() => {
    loadCategories();
  }, []);

  useEffect(() => {
    loadItems();
  }, [filterCategoryId]);

  const loadCategories = async () => {
    try {
      const data = await itemCategoryService.getAll();
      setCategories(data);
    } catch (err: any) {
      console.error("Failed to load categories", err);
    }
  };

  const loadItems = async () => {
    setLoading(true);
    setError(null);
    try {
      const companyId = getCompanyId();
      const data = await itemService.getAll({
        companyId,
        categoryId: filterCategoryId || undefined,
      });
      setItems(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load items");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this item?")) return;

    try {
      await itemService.delete(id);
      loadItems();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete item");
    }
  };

  const getItemTypeLabel = (itemType?: string) => {
    const option = ITEM_TYPE_OPTIONS.find((opt) => opt.value === itemType);
    return option?.label || itemType || "N/A";
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("itemName");
  const sortedItems = useMemo(
    () => sortRows(items),
    [items, sortRows]
  );

  return (
    <>
      <PageBreadcrumb
        title="Items"
        name="Inventory"
        breadCrumbItems={["Inventory", "Items"]}
      />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Items</h4>
            <Link to="/inventory/items/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Item
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4">
            <select
              className="form-select"
              value={filterCategoryId}
              onChange={(e) => setFilterCategoryId(e.target.value)}
            >
              <option value="">All Categories</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.code} - {cat.name}
                </option>
              ))}
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
                <SortableTh columnKey="itemCode" label="Item Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="itemName" label="Item Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="itemCategoryName" label="Category" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="itemType" label="Item Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="unitOfMeasureName" label="Unit of Measure" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="costPrice" label="Cost Price" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="sellingPrice" label="Selling Price" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {items.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No items found
                      </td>
                    </tr>
                  ) : (
                    sortedItems.map((item) => (
                      <tr key={item.id}>
                        <td>{item.itemCode}</td>
                        <td>{item.itemName}</td>
                        <td>{item.itemCategoryName || item.categoryName || "-"}</td>
                        <td>
                          <span className="badge bg-info">{getItemTypeLabel(item.itemType)}</span>
                        </td>
                        <td>{item.unitOfMeasureName || "-"}</td>
                        <td>{item.costPrice.toFixed(2)}</td>
                        <td>{item.sellingPrice.toFixed(2)}</td>
                        <td>
                          <span
                            className={`badge badge-pill ${
                              item.isActive ? "badge-soft-success" : "badge-soft-danger"
                            }`}
                          >
                            {item.isActive ? "Active" : "Inactive"}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/inventory/items/${item.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(item.id || "")}
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

export default ItemList;

