import React, { useEffect, useState , useMemo} from "react";
import { useTableSort, SortableTh } from "../../../../components";
import { Link, useNavigate } from "react-router-dom";
import { PageBreadcrumb } from "../../../../components";
// Use Core module service to avoid duplication
import { userService, type UserDto } from "../../../../modules/core/entities/user/services";
import Swal from "sweetalert2";

const UserList = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterActive, setFilterActive] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    loadUsers();
  }, [filterActive]);

  const loadUsers = async () => {
    try {
      setLoading(true);
      const data = await userService.getAll(undefined, filterActive);
      setUsers(data);
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to load users");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string, name: string) => {
    const result = await Swal.fire({
      title: "Are you sure?",
      text: `Do you want to delete user ${name}?`,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Yes, delete it!",
    });

    if (result.isConfirmed) {
      try {
        await userService.delete(id);
        Swal.fire("Deleted!", "User has been deleted.", "success");
        loadUsers();
      } catch (err: any) {
        Swal.fire("Error!", err.response?.data?.Error || err.response?.data?.message || "Failed to delete user", "error");
      }
    }
  };

  const handleToggleActive = async (user: UserDto) => {
    try {
      if (user.isActive) {
        await userService.deactivate(user.id);
      } else {
        await userService.activate(user.id);
      }
      loadUsers();
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to update user status");
    }
  };

  const { sortKey, sortDir, toggleSort, sortRows } = useTableSort("fullName");
  const sortedUsers = useMemo(
    () => sortRows(users),
    [users, sortRows]
  );

  return (
    <>
      <PageBreadcrumb title="Users" subName="Admin" />
      <div className="card">
        <div className="card-header flex justify-between items-center">
          <h4 className="card-title">Users</h4>
          <Link to="/erp/admin/users/new" className="btn btn-primary">
            <i className="mgc_add_line me-2"></i>Add User
          </Link>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <div className="mb-4 flex gap-2">
            <button
              onClick={() => setFilterActive(undefined)}
              className={`btn btn-sm ${filterActive === undefined ? 'bg-primary text-white' : 'btn-soft-primary'}`}
            >
              All
            </button>
            <button
              onClick={() => setFilterActive(true)}
              className={`btn btn-sm ${filterActive === true ? 'bg-success text-white' : 'btn-soft-success'}`}
            >
              Active
            </button>
            <button
              onClick={() => setFilterActive(false)}
              className={`btn btn-sm ${filterActive === false ? 'bg-danger text-white' : 'btn-soft-danger'}`}
            >
              Inactive
            </button>
          </div>

          {loading ? (
            <div className="text-center py-8">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="table table-hover table-nowrap mb-0 w-full">
                <thead>
                <tr>
                <SortableTh columnKey="fullName" label="Name" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="email" label="Email" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="phone" label="Phone" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="status" label="Status" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-start p-3 font-semibold" />
                <SortableTh columnKey="_Actions" label="Actions" sortKey={sortKey} sortDir={sortDir} onSort={toggleSort} className="text-end p-3 font-semibold" sortable={false} />
                </tr>
              </thead>
                <tbody>
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="text-center p-8 text-gray-500">
                        No users found
                      </td>
                    </tr>
                  ) : (
                    sortedUsers.map((user) => (
                      <tr key={user.id} className="border-b border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800">
                        <td className="p-3">{`${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email}</td>
                        <td className="p-3">{user.email}</td>
                        <td className="p-3 text-gray-500">{user.phone || "-"}</td>
                        <td className="p-3">
                          <span className="text-gray-400">-</span>
                        </td>
                        <td className="p-3">
                          <span className={`badge ${user.isActive ? 'bg-success' : 'bg-secondary'}`}>
                            {user.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td className="p-3">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => navigate(`/erp/admin/users/${user.id}/edit`)}
                              className="btn btn-sm btn-soft-warning"
                              title="Edit"
                            >
                              <i className="mgc_edit_line"></i>
                            </button>
                            <button
                              onClick={() => handleToggleActive(user)}
                              className={`btn btn-sm ${user.isActive ? 'btn-soft-danger' : 'btn-soft-success'}`}
                              title={user.isActive ? 'Deactivate' : 'Activate'}
                            >
                              <i className={user.isActive ? 'mgc_close_line' : 'mgc_check_line'}></i>
                            </button>
                            <Link
                              to={`/erp/admin/users/${user.id}/reset-password`}
                              className="btn btn-sm btn-soft-info"
                              title="Reset Password"
                            >
                              <i className="mgc_key_line"></i>
                            </Link>
                            <button
                              onClick={() => handleDelete(user.id, `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email)}
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

export default UserList;

