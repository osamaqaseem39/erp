import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { priceListService, type PriceListDto } from "../services";
import Swal from "sweetalert2";

const PriceListList: React.FC = () => {
  const [priceLists, setPriceLists] = useState<PriceListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPriceLists();
  }, []);

  const loadPriceLists = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await priceListService.getAll();
      setPriceLists(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load price lists");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete price list ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await priceListService.delete(id);
        Swal.fire("Deleted!", "Price list has been deleted.", "success");
        loadPriceLists();
      } catch (err: any) {
        Swal.fire(
          "Error!",
          err.response?.data?.Error || err.response?.data?.message || "Failed to delete price list",
          "error"
        );
      }
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString();
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedPriceLists = useMemo(
    () => sortRows(priceLists),
    [priceLists, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Price Lists" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Price Lists</h4>
            <Link to="/crm/price-lists/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Price List
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
            <div className="text-center py-8">Loading...</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover table-nowrap mb-0">
                <thead>
                <tr>
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="validFrom" label="Valid From" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="validTo" label="Valid To" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isDefault" label="Default" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {priceLists.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center py-8">
                        No price lists found
                      </td>
                    </tr>
                  ) : (
                    sortedPriceLists.map((priceList) => (
                      <tr key={priceList.id}>
                        <td>{priceList.priceListName || "-"}</td>
                        <td>{priceList.description || "-"}</td>
                        <td>{formatDate(priceList.validFrom)}</td>
                        <td>{formatDate(priceList.validTo)}</td>
                        <td>
                          {priceList.isDefault ? (
                            <span className="badge bg-success">Yes</span>
                          ) : (
                            <span className="badge bg-secondary">No</span>
                          )}
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/price-lists/${priceList.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(priceList.id || "", priceList.priceListName || "")}
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

export default PriceListList;
