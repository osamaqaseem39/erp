import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { tenantService, type CreateTenantDto, type UpdateTenantDto, type TenantDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const TenantForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const tenantSchema = yup.object({
    name: yup.string().required("Tenant name is required"),
    tenantName: yup.string(),
    domain: yup.string().required("Domain is required"),
    industry: yup.string(),
    country: yup.string(),
    status: yup.string(),
    databaseName: yup.string(),
    connectionString: yup.string(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [tenant, setTenant] = useState<TenantDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadTenant(id);
    }
  }, [isEdit, id]);

  const loadTenant = async (tenantId: string) => {
    try {
      const tenantData = await tenantService.getById(tenantId);
      setTenant(tenantData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load tenant");
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateTenantDto = {
          id,
          name: data.name,
          tenantName: data.tenantName,
          domain: data.domain,
          industry: data.industry,
          country: data.country,
          status: data.status || "Active",
          databaseName: data.databaseName,
          connectionString: data.connectionString,
          isActive: data.isActive ?? true,
        };
        await tenantService.update(id, updateData);
      } else {
        const createData: CreateTenantDto = {
          name: data.name,
          tenantName: data.tenantName,
          domain: data.domain,
          industry: data.industry,
          country: data.country,
          status: data.status || "Active",
          databaseName: data.databaseName,
          connectionString: data.connectionString,
          isActive: data.isActive ?? true,
        };
        await tenantService.create(createData);
      }
      navigate("/core/settings/tenants");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add Tenant"} tenant`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Tenant" : "Add Tenant"} subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/core/settings/tenants" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit Tenant" : "Add Tenant"}</h4>
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
            resolver={yupResolver(tenantSchema)}
            defaultValues={tenant || {
              name: "",
              tenantName: "",
              domain: "",
              industry: "",
              country: "",
              status: "Active",
              databaseName: "",
              connectionString: "",
              isActive: true,
            }}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Name"
                type="text"
                name="name"
                placeholder="Enter tenant name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Tenant Name"
                type="text"
                name="tenantName"
                placeholder="Enter tenant display name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Domain"
                type="text"
                name="domain"
                placeholder="Enter domain"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Industry"
                type="text"
                name="industry"
                placeholder="Enter industry"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Country"
                type="text"
                name="country"
                placeholder="Enter country"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Suspended">Suspended</option>
              </FormInput>

              <FormInput
                label="Database Name"
                type="text"
                name="databaseName"
                placeholder="Enter database name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Connection String"
                type="text"
                name="connectionString"
                placeholder="Enter connection string"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />
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
              <Link to="/core/settings/tenants" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Tenant"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default TenantForm;

