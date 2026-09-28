import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { userService, type CreateUserDto, type UpdateUserDto, type UserDto } from "../services";
import { roleService, type RoleDto } from "../../role/services";
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
    password: isEdit ? yup.string() : yup.string().min(6, "Password must be at least 6 characters").required("Password is required"),
    phone: yup.string(),
    isActive: yup.boolean(),
    roleIds: yup.array().of(yup.string()),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [user, setUser] = useState<UserDto | null>(null);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);

  useEffect(() => {
    loadRoles();
    if (isEdit && id) {
      loadUser(id);
    }
  }, [isEdit, id]);

  const loadUser = async (userId: string) => {
    try {
      const userData = await userService.getById(userId);
      setUser(userData);
      // Load user roles if available
      // setSelectedRoles(userData.roleIds || []);
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

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateUserDto = {
          id,
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phone: data.phone,
          isActive: data.isActive ?? true,
          roleIds: selectedRoles,
        };
        await userService.update(id, updateData);
      } else {
        const createData: CreateUserDto = {
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          password: data.password,
          phone: data.phone,
          isActive: data.isActive ?? true,
          roleIds: selectedRoles,
        };
        await userService.create(createData);
      }
      navigate("/core/settings/users");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add User"} user`);
    } finally {
      setLoading(false);
    }
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

          <VerticalForm
            onSubmit={onSubmit}
            resolver={yupResolver(userSchema)}
            defaultValues={user || {
              firstName: "",
              lastName: "",
              email: "",
              phone: "",
              isActive: true,
            }}
          >
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
                label="Phone"
                type="text"
                name="phone"
                placeholder="Enter phone number"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2">
                  Roles
                </label>
                <select
                  multiple
                  className="form-select"
                  value={selectedRoles}
                  onChange={(e) => {
                    const values = Array.from(e.target.selectedOptions, option => option.value);
                    setSelectedRoles(values);
                  }}
                >
                  {roles.map((role) => (
                    <option key={role.id} value={role.id}>
                      {role.name}
                    </option>
                  ))}
                </select>
                <small className="text-gray-500">Hold Ctrl/Cmd to select multiple roles</small>
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
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add User"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default UserForm;

