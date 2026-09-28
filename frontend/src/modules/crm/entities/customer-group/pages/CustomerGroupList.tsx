import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { customerGroupService, type CustomerGroupDto } from "../services";
import Swal from "sweetalert2";

const CustomerGroupList: React.FC = () => {
  const [customerGroups, setCustomerGroups] = useState<CustomerGroupDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCustomerGroups();
  }, []);

  const loadCustomerGroups = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await customerGroupService.getAll();
      setCustomerGroups(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load customer groups");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete customer group ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await customerGroupService.delete(id);
        Swal.fire("Deleted!", "Customer group has been deleted.", "success");
        loadCustomerGroups();
      } catch (err: any) {
        Swal.fire(
          "Error!",
          err.response?.data?.Error || err.response?.data?.message || "Failed to delete customer group",
          "error"
        );
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("groupName");
  const sortedCustomerGroups = useMemo(
    () => sortRows(customerGroups),
    [customerGroups, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Customer Groups" subName="CRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Customer Groups</h4>
            <Link to="/crm/customer-groups/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Customer Group
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
                <SortableTh columnKey="groupName" label="Group Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="discountPercentage" label="Discount %" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {customerGroups.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center py-8">
                        No customer groups found
                      </td>
                    </tr>
                  ) : (
                    sortedCustomerGroups.map((group) => (
                      <tr key={group.id}>
                        <td>{group.groupName || "-"}</td>
                        <td>{group.description || "-"}</td>
                        <td>{group.discountPercentage != null ? `${group.discountPercentage}%` : "-"}</td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/crm/customer-groups/${group.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(group.id || "", group.groupName || "")}
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

export default CustomerGroupList;
