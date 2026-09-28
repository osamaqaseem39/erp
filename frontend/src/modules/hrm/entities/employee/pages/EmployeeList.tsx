import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../../components";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { employeeService, type EmployeeListDto } from "../services";
import Swal from "sweetalert2";

const EmployeeList: React.FC = () => {
  const [employees, setEmployees] = useState<EmployeeListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");

  useEffect(() => {
    loadEmployees();
  }, [statusFilter]);

  const loadEmployees = async () => {
    try {
      setLoading(true);
      const query: any = {};
      if (searchTerm) query.searchTerm = searchTerm;
      if (statusFilter) query.status = statusFilter;
      const data = await employeeService.getAll(query);
      setEmployees(data);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load employees");
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    loadEmployees();
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete employee ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await employeeService.delete(id);
        Swal.fire("Deleted!", "Employee has been deleted.", "success");
        loadEmployees();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete employee", "error");
      }
    }
  };

  const filteredEmployees = employees.filter((emp) => {
    if (searchTerm) {
      const search = searchTerm.toLowerCase();
      return (
        emp.employeeCode.toLowerCase().includes(search) ||
        emp.firstName.toLowerCase().includes(search) ||
        emp.lastName.toLowerCase().includes(search) ||
        emp.jobTitle?.toLowerCase().includes(search) ||
        emp.department?.toLowerCase().includes(search) ||
        emp.workEmail?.toLowerCase().includes(search)
      );
    }
    return true;
  });

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("firstName");
  const sortedEmployees = useMemo(
    () => sortRows(filteredEmployees),
    [filteredEmployees, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Employees" subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center justify-between">
            <h4 className="card-title">Employees</h4>
            <Link to="/hrm/employees/new" className="btn btn-primary">
              <i className="mgc_add_line me-2"></i>Add Employee
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {/* Filters */}
          <div className="mb-4 flex items-center gap-4">
            <div className="flex-1">
              <input
                type="text"
                className="form-input"
                placeholder="Search by code, name, job title, department, or email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={(e) => e.key === "Enter" && handleSearch()}
              />
            </div>
            <div>
              <select
                className="form-select"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="">All Status</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Terminated">Terminated</option>
              </select>
            </div>
            <button onClick={handleSearch} className="btn btn-soft-primary">
              <i className="mgc_search_line me-2"></i>Search
            </button>
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
                <SortableTh columnKey="employeeCode" label="Code" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="firstName" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="jobTitle" label="Job Title" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="department" label="Department" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="workEmail" label="Work Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="workPhone" label="Work Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="dateHired" label="Date Hired" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {filteredEmployees.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="text-center py-8">
                        No employees found
                      </td>
                    </tr>
                  ) : (
                    sortedEmployees.map((employee) => (
                      <tr key={employee.id}>
                        <td>{employee.employeeCode}</td>
                        <td>{employee.firstName} {employee.lastName}</td>
                        <td>{employee.jobTitle || "-"}</td>
                        <td>{employee.department || "-"}</td>
                        <td>{employee.workEmail || "-"}</td>
                        <td>{employee.workPhone || "-"}</td>
                        <td>{employee.dateHired ? new Date(employee.dateHired).toLocaleDateString() : "-"}</td>
                        <td>
                          <span className={`badge ${
                            employee.status === "Active" ? "bg-success" :
                            employee.status === "Inactive" ? "bg-warning" :
                            "bg-danger"
                          }`}>
                            {employee.status}
                          </span>
                        </td>
                        <td>
                          <div className="flex items-center gap-2">
                            <Link
                              to={`/hrm/employees/${employee.id}/edit`}
                              className="btn btn-sm btn-soft-primary"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(employee.id, `${employee.firstName} ${employee.lastName}`)}
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

export default EmployeeList;

