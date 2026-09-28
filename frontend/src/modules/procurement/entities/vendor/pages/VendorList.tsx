import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { vendorService, type VendorDto } from "../services";
import { VENDOR_STATUS_OPTIONS, VENDOR_TYPE_OPTIONS } from "../types";

const VendorList: React.FC = () => {
  const [vendors, setVendors] = useState<VendorDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState<string>("");

  useEffect(() => {
    loadVendors();
  }, [filterStatus, searchTerm]);

  const loadVendors = async () => {
    setLoading(true);
    setError(null);
    try {
      const isActive = filterStatus === "Active" ? true : filterStatus === "Inactive" ? false : undefined;
      const data = await vendorService.getAll(searchTerm || undefined, undefined, isActive);
      
      // If status filter is not Active/Inactive, filter client-side
      let filtered = data;
      if (filterStatus && filterStatus !== "Active" && filterStatus !== "Inactive") {
        filtered = data.filter((vendor) => vendor.status === filterStatus);
      }
      
      setVendors(filtered);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load vendors");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this vendor?")) return;

    try {
      await vendorService.delete(id);
      loadVendors();
    } catch (err: any) {
      alert(err.response?.data?.Error || err.response?.data?.message || "Failed to delete vendor");
    }
  };

  const getStatusLabel = (status?: string) => {
    const option = VENDOR_STATUS_OPTIONS.find((opt) => opt.value === status);
    return option?.label || status || "N/A";
  };

  const getStatusBadgeClass = (status?: string) => {
    switch (status) {
      case "Active":
        return "bg-success";
      case "Inactive":
        return "bg-secondary";
      case "Suspended":
        return "bg-danger";
      case "Pending":
        return "bg-warning";
      default:
        return "bg-secondary";
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("vendorName");
  const sortedVendors = useMemo(
    () => sortRows(vendors),
    [vendors, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Vendors" subName="Procurement" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Vendors</h4>
            <Link to="/procurement/vendors/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Vendor
            </Link>
          </div>
        </div>
        <div className="card-body">
          {/* Filters */}
          <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
            <input
              type="text"
              className="form-input"
              placeholder="Search vendors..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />

            <select
              className="form-select"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">All Status</option>
              {VENDOR_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
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
                <SortableTh columnKey="vendorCode" label="Vendor Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="vendorName" label="Vendor Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="vendorType" label="Type" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="contactPerson" label="Contact Person" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="city" label="City" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="creditLimit" label="Credit Limit" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {vendors.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="text-center py-8">
                        No vendors found
                      </td>
                    </tr>
                  ) : (
                    sortedVendors.map((vendor) => (
                      <tr key={vendor.id}>
                        <td>{vendor.vendorCode}</td>
                        <td>{vendor.vendorName}</td>
                        <td>{vendor.vendorType || "-"}</td>
                        <td>{vendor.contactPerson || "-"}</td>
                        <td>{vendor.email || "-"}</td>
                        <td>{vendor.phone || vendor.phoneNumber || "-"}</td>
                        <td>{vendor.city || "-"}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(vendor.status)}`}>
                            {getStatusLabel(vendor.status)}
                          </span>
                        </td>
                        <td>{vendor.creditLimit ? vendor.creditLimit.toFixed(2) : "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/procurement/vendors/${vendor.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            {vendor.status === "Draft" && (
                              <button
                                onClick={() => handleDelete(vendor.id || "")}
                                className="btn btn-sm btn-soft-danger"
                              >
                                <i className="mgc_delete_line"></i>
                              </button>
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

export default VendorList;

