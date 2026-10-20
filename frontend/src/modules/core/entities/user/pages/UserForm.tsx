import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { userService, type CreateUserDto, type UpdateUserDto, type UserDto } from "../services";
import { roleService, type RoleDto } from "../../role/services";
import { employeeService } from "../../../../hrm/entities/employee/services";
import type { EmployeeListDto } from "../../../../hrm/entities/employee/types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const UserForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const userSchema = yup.object({
    firstName: yup.string().required("First name is required"),
    lastName: yup.string().required("Last name is required"),
    email: yup.string().email("Invalid email").required("Email is required"),
    username: yup.string().max(100),
    password: isEdit
      ? yup.string()
      : yup.string().min(6, "Password must be at least 6 characters").required("Password is required"),
    phone: yup.string(),
    secondaryPhone: yup.string(),
    mobilePhone: yup.string(),
    addressLine1: yup.string().max(200),
    city: yup.string().max(100),
    profilePicture: yup
      .string()
      .url("Profile picture must be a valid URL")
      .nullable()
      .transform((v) => (v === "" ? null : v)),
    isActive: yup.boolean(),
    roleIds: yup.array().of(yup.string()).min(1, "Assign at least one role (user type)"),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeListDto[]>([]);
  const [user, setUser] = useState<UserDto | null>(null);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [employeeId, setEmployeeId] = useState<string>("");

  useEffect(() => {
    loadRoles();
    loadEmployees();
    if (isEdit && id) {
      loadUser(id);
    }
  }, [isEdit, id]);

  const loadUser = async (userId: string) => {
    try {
      const userData = await userService.getById(userId);
      setUser(userData);
      setSelectedRoles(userData.roleIds || []);
      setEmployeeId(userData.employeeId || "");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load user");
    }
  };

  const loadRoles = async () => {
    try {
      const data = await roleService.getAll(true);
      setRoles(data);
    } catch (err: any) {
      console.error("Failed to load roles", err);
    }
  };

  const loadEmployees = async () => {
    try {
      const data = await employeeService.getAll({ pageSize: 200, pageNumber: 1 });
      const rows = Array.isArray(data) ? data : (data as any)?.items || (data as any)?.Items || [];
      setEmployees(rows);
    } catch (err: any) {
      // HRM may be unavailable; picker stays empty.
      console.warn("Failed to load employees for user link", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    if (selectedRoles.length === 0) {
      setError("Assign at least one role (user type).");
      return;
    }
    setLoading(true);
    try {
      const shared = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        username: data.username || undefined,
        phone: data.phone,
        secondaryPhone: data.secondaryPhone,
        mobilePhone: data.mobilePhone,
        addressLine1: data.addressLine1,
        city: data.city,
        profilePicture: data.profilePicture || undefined,
        employeeId: employeeId || undefined,
        isActive: data.isActive ?? true,
        roleIds: selectedRoles,
      };
      if (isEdit && id) {
        const updateData: UpdateUserDto = { id, ...shared };
        await userService.update(id, updateData);
      } else {
        const createData: CreateUserDto = {
          ...shared,
          password: data.password,
        };
        await userService.create(createData);
      }
      navigate("/core/settings/users");
    } catch (err: any) {
      setError(
        err.response?.data?.Error ||
          err.response?.data?.message ||
          `Failed to ${isEdit ? "save" : "create"} user`
      );
    } finally {
      setLoading(false);
    }
  };

  const defaults = user || {
    firstName: "",
    lastName: "",
    email: "",
    username: "",
    phone: "",
    secondaryPhone: "",
    mobilePhone: "",
    addressLine1: "",
    city: "",
    profilePicture: "",
    isActive: true,
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit User" : "Add User"} subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/core/settings/users" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit User" : "Add User"}</h4>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          {(isEdit ? user : true) && (
            <VerticalForm onSubmit={onSubmit} resolver={yupResolver(userSchema)} defaultValues={defaults}>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="First Name"
                  type="text"
                  name="firstName"
                  placeholder="Enter first name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Last Name"
                  type="text"
                  name="lastName"
                  placeholder="Enter last name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="Enter email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Username"
                  type="text"
                  name="username"
                  placeholder="Defaults to email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                {!isEdit && (
                  <FormInput
                    label="Password"
                    type="password"
                    name="password"
                    placeholder="Enter password"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    required
                  />
                )}

                <FormInput
                  label="Primary phone"
                  type="text"
                  name="phone"
                  placeholder="Primary phone"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Secondary phone"
                  type="text"
                  name="secondaryPhone"
                  placeholder="Secondary phone"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Mobile"
                  type="text"
                  name="mobilePhone"
                  placeholder="Mobile phone"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Address"
                  type="text"
                  name="addressLine1"
                  placeholder="Street address"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="City"
                  type="text"
                  name="city"
                  placeholder="City"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Profile picture URL"
                  type="url"
                  name="profilePicture"
                  placeholder="https://?"
                  containerClass="mb-4 md:col-span-2"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <div className="mb-4 md:col-span-2">
                  <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2" htmlFor="user-employee">
                    Linked employee
                  </label>
                  <select
                    id="user-employee"
                    className="form-select"
                    value={employeeId}
                    onChange={(e) => setEmployeeId(e.target.value)}
                  >
                    <option value="">? None ?</option>
                    {employees.map((emp) => {
                      const eid = String(emp.id || emp.employeeId || "");
                      return (
                        <option key={eid} value={eid}>
                          {emp.employeeCode} ? {emp.firstName} {emp.lastName}
                        </option>
                      );
                    })}
                  </select>
                  <small className="text-gray-500">Optional. Links this login to an HRM employee record.</small>
                </div>

                <div className="mb-4 md:col-span-2">
                  <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2">
                    Roles (user type) <span className="text-danger">*</span>
                  </label>
                  <select
                    multiple
                    className="form-select"
                    value={selectedRoles}
                    onChange={(e) => {
                      const values = Array.from(e.target.selectedOptions, (option) => option.value);
                      setSelectedRoles(values);
                    }}
                    required
                  >
                    {roles.map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                  </select>
                  <small className="text-gray-500">
                    Required. Hold Ctrl/Cmd to select multiple. Examples: Accountant, Warehouse, Sales.
                  </small>
                </div>
              </div>

              <div className="flex items-center gap-4 mb-6">
                <FormInput
                  label="Active"
                  type="checkbox"
                  name="isActive"
                  containerClass="flex items-center"
                  labelClassName="ms-2"
                  className="form-checkbox rounded"
                />
              </div>

              <div className="flex justify-end gap-2">
                <Link to="/core/settings/users" className="btn btn-soft-secondary">
                  Cancel
                </Link>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? "Saving?" : isEdit ? "Save changes" : "Add User"}
                </button>
              </div>
            </VerticalForm>
          )}
        </div>
      </div>
    </>
  );
};

export default UserForm;
