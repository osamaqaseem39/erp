import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { warehouseService, type CreateWarehouseDto, type UpdateWarehouseDto, type WarehouseDto } from "../services";
import { employeeService } from "../../../../hrm/entities/employee/services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const WarehouseForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const warehouseSchema = yup.object({
    warehouseCode: yup.string().required("Warehouse code is required"),
    warehouseName: yup.string().required("Warehouse name is required"),
    description: yup.string(),
    addressLine1: yup.string().required("Address line 1 is required"),
    addressLine2: yup.string(),
    city: yup.string().required("City is required"),
    state: yup.string().required("State is required"),
    country: yup.string().required("Country is required"),
    postalCode: yup.string().required("Postal code is required"),
    phoneNumber: yup.string(),
    email: yup.string().email("Invalid email"),
    managerId: yup.string().uuid("Invalid manager").nullable(),
    contactPerson: yup.string(),
    isDefault: yup.boolean(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [warehouse, setWarehouse] = useState<WarehouseDto | null>(null);
  const [employees, setEmployees] = useState<any[]>([]);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadWarehouse(id);
    }
  }, [isEdit, id]);

  const loadWarehouse = async (warehouseId: string) => {
    try {
      const warehouseData = await warehouseService.getById(warehouseId);
      setWarehouse(warehouseData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load warehouse");
    }
  };

  const loadEmployees = async () => {
    try {
      const data = await employeeService.getAll();
      setEmployees(data);
    } catch (err: any) {
      console.error("Failed to load employees", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateWarehouseDto = {
          id,
          warehouseCode: data.warehouseCode,
          warehouseName: data.warehouseName,
          description: data.description || undefined,
          addressLine1: data.addressLine1,
          addressLine2: data.addressLine2 || undefined,
          city: data.city,
          state: data.state,
          country: data.country,
          postalCode: data.postalCode,
          phoneNumber: data.phoneNumber || undefined,
          email: data.email || undefined,
          managerId: data.managerId || undefined,
          contactPerson: data.contactPerson || undefined,
          isDefault: data.isDefault ?? false,
          isActive: data.isActive ?? true,
        };
        await warehouseService.update(id, updateData);
      } else {
        const createData: CreateWarehouseDto = {
          companyId: data.companyId || "", // TODO: Get from context/state
          warehouseCode: data.warehouseCode,
          warehouseName: data.warehouseName,
          description: data.description || undefined,
          addressLine1: data.addressLine1,
          addressLine2: data.addressLine2 || undefined,
          city: data.city,
          state: data.state,
          country: data.country,
          postalCode: data.postalCode,
          phoneNumber: data.phoneNumber || undefined,
          email: data.email || undefined,
          managerId: data.managerId || undefined,
          contactPerson: data.contactPerson || undefined,
          isDefault: data.isDefault ?? false,
          isActive: data.isActive ?? true,
        };
        await warehouseService.create(createData);
      }
      navigate("/inventory/warehouses");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save warehouse");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && warehouse
    ? {
        warehouseCode: warehouse.warehouseCode || "",
        warehouseName: warehouse.warehouseName || "",
        description: warehouse.description || "",
        addressLine1: warehouse.addressLine1 || "",
        addressLine2: warehouse.addressLine2 || "",
        city: warehouse.city || "",
        state: warehouse.state || "",
        country: warehouse.country || "",
        postalCode: warehouse.postalCode || "",
        phoneNumber: warehouse.phoneNumber || "",
        email: warehouse.email || "",
        managerId: warehouse.managerId || "",
        contactPerson: warehouse.contactPerson || "",
        isDefault: warehouse.isDefault ?? false,
        isActive: warehouse.isActive ?? true,
      }
    : {
        warehouseCode: "",
        warehouseName: "",
        description: "",
        addressLine1: "",
        addressLine2: "",
        city: "",
        state: "",
        country: "",
        postalCode: "",
        phoneNumber: "",
        email: "",
        managerId: "",
        contactPerson: "",
        isDefault: false,
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Warehouse" : "Add Warehouse"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/warehouses" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
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
            resolver={yupResolver(warehouseSchema)}
            defaultValues={defaultValues}
          >
            {/* Basic Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Basic Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Warehouse Code"
                  type="text"
                  name="warehouseCode"
                  placeholder="Enter warehouse code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Warehouse Name"
                  type="text"
                  name="warehouseName"
                  placeholder="Enter warehouse name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Manager"
                  type="select"
                  name="managerId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select manager</option>
                  {employees.map((emp) => (
                    <option key={emp.id} value={emp.id}>
                      {emp.firstName} {emp.lastName} ({emp.employeeCode})
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Contact Person"
                  type="text"
                  name="contactPerson"
                  placeholder="Enter contact person"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <div className="flex items-center gap-6">
                  <FormInput
                    label="Is Default"
                    type="checkbox"
                    name="isDefault"
                    containerClass="mb-4"
                    className="form-checkbox"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />

                  <FormInput
                    label="Is Active"
                    type="checkbox"
                    name="isActive"
                    containerClass="mb-4"
                    className="form-checkbox"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            {/* Address Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Address Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <div className="md:col-span-2">
                  <FormInput
                    label="Address Line 1"
                    type="text"
                    name="addressLine1"
                    placeholder="Enter address line 1"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    required
                  />
                </div>

                <div className="md:col-span-2">
                  <FormInput
                    label="Address Line 2"
                    type="text"
                    name="addressLine2"
                    placeholder="Enter address line 2"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>

                <FormInput
                  label="City"
                  type="text"
                  name="city"
                  placeholder="Enter city"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="State"
                  type="text"
                  name="state"
                  placeholder="Enter state"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Country"
                  type="text"
                  name="country"
                  placeholder="Enter country"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Postal Code"
                  type="text"
                  name="postalCode"
                  placeholder="Enter postal code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />
              </div>
            </div>

            {/* Contact Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Contact Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Phone Number"
                  type="text"
                  name="phoneNumber"
                  placeholder="Enter phone number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="Enter email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            {/* Description */}
            <div className="mb-6">
              <FormInput
                label="Description"
                type="textarea"
                name="description"
                rows={4}
                placeholder="Enter warehouse description"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/warehouses" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Warehouse"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default WarehouseForm;

