import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { roleService, type CreateRoleDto, type UpdateRoleDto, type RoleDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const RoleForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const roleSchema = yup.object({
    name: yup.string().required("Role name is required"),
    description: yup.string(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [role, setRole] = useState<RoleDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadRole(id);
    }
  }, [isEdit, id]);

  const loadRole = async (roleId: string) => {
    try {
      const roleData = await roleService.getById(roleId);
      setRole(roleData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load role");
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateRoleDto = {
          id,
          name: data.name,
          description: data.description,
          isActive: data.isActive ?? true,
        };
        await roleService.update(id, updateData);
      } else {
        const createData: CreateRoleDto = {
          name: data.name,
          description: data.description,
          isActive: data.isActive ?? true,
        };
        await roleService.create(createData);
      }
      navigate("/core/settings/roles");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add Role"} role`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Role" : "Add Role"} subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/core/settings/roles" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit Role" : "Add Role"}</h4>
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
            resolver={yupResolver(roleSchema)}
            defaultValues={role || {
              name: "",
              description: "",
              isActive: true,
            }}
          >
            <FormInput
              label="Role Name"
              type="text"
              name="name"
              placeholder="Enter role name"
              containerClass="mb-4"
              className="form-input"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
            />

            <FormInput
              label="Description"
              type="textarea"
              name="description"
              placeholder="Enter role description"
              containerClass="mb-4"
              className="form-input"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              rows={4}
            />

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
              <Link to="/core/settings/roles" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Role"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default RoleForm;

