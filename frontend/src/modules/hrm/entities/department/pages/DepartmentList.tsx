import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { departmentService, type DepartmentDto } from "../services";
import Swal from "sweetalert2";

const DepartmentList: React.FC = () => {
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDepartments();
  }, []);

  const loadDepartments = async () => {
    try {
      setLoading(true);
      const data = await departmentService.getAll();
      setDepartments(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load departments");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete department ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await departmentService.delete(id!);
        Swal.fire("Deleted!", "Department has been deleted.", "success");
        loadDepartments();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete department", "error");
      }
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("name");
  const sortedDepartments = useMemo(
    () => sortRows(departments),
    [departments, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Departments" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Departments</h4>
            <Link to="/hrm/departments/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Department
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
                <SortableTh columnKey="name" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="description" label="Description" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="managerName" label="Manager" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="parentDepartmentName" label="Parent Department" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="isActive" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {departments.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-center py-8">
                        No departments found
                      </td>
                    </tr>
                  ) : (
                    sortedDepartments.map((dept) => (
                      <tr key={dept.id}>
                        <td>{dept.name}</td>
                        <td>{dept.description || "-"}</td>
                        <td>{dept.managerId || "-"}</td>
                        <td>{dept.parentDepartmentId || "-"}</td>
                        <td>
                          <span
                            className={`badge badge-pill ${
                              dept.isActive ? "badge-soft-success" : "badge-soft-danger"
                            }`}
                          >
                            {dept.isActive ? "Active" : "Inactive"}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/departments/${dept.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(dept.id!, dept.name)}
                              className="btn btn-sm btn-soft-danger"
                              title="Delete"
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

export default DepartmentList;

