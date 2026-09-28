import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../components";
import { VerticalForm, FormInput } from "../../../components";
// Use Core module service to avoid duplication
import { tenantService, type CreateTenantDto, type UpdateTenantDto, type TenantDto } from "../../../modules/core/entities/tenant/services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const tenantSchema = yup.object({
  name: yup.string().required("Name is required"),
  tenantName: yup.string(),
  domain: yup.string().required("Domain is required"),
  industry: yup.string().required("Industry is required"),
  country: yup.string().required("Country is required"),
  status: yup.string().required("Status is required"),
  databaseName: yup.string().required("Database name is required"),
  connectionString: yup.string().required("Connection string is required"),
  isActive: yup.boolean(),
});

type TenantFormValues = yup.InferType<typeof tenantSchema>;

const industries = ["Technology", "Finance", "Healthcare", "Retail", "Manufacturing", "Education", "Real Estate", "Other"];
const countries = ["United States", "United Kingdom", "Canada", "Australia", "Germany", "France", "India", "Other"];
const statuses = ["Active", "Inactive", "Suspended", "Pending"];

const TenantForm = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEdit && id) {
      loadTenant(id);
    }
  }, [isEdit, id]);

  const loadTenant = async (tenantId: string) => {
    try {
      const tenant = await tenantService.getById(tenantId);
      // Form will be populated by defaultValues
    } catch (err: any) {
      setError(err.response?.data?.message || "Failed to load tenant");
    }
  };

  const onSubmit = async (data: TenantFormValues) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        await tenantService.update(id, data as UpdateTenantDto);
      } else {
        await tenantService.create(data as CreateTenantDto);
      }
      navigate("/erp/tenants");
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add "} tenant`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Tenant" : "Add Tenant"} subName="ERP" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/erp/tenants" className="btn btn-sm btn-soft-secondary">
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

          <VerticalForm<TenantFormValues>
            onSubmit={onSubmit}
            resolver={yupResolver(tenantSchema)}
            defaultValues={{
              status: "Active",
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
                placeholder="Enter tenant name"
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
                type="select"
                name="industry"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select Industry</option>
                {industries.map((industry) => (
                  <option key={industry} value={industry}>
                    {industry}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Country"
                type="select"
                name="country"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select Country</option>
                {countries.map((country) => (
                  <option key={country} value={country}>
                    {country}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                {statuses.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Database Name"
                type="text"
                name="databaseName"
                placeholder="Enter database name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Connection String"
                type="textarea"
                name="connectionString"
                placeholder="Enter connection string"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                rows={2}
                required
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
              <Link to="/erp/tenants" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add "}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default TenantForm;

