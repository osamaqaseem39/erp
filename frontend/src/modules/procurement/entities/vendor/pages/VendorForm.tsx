import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { vendorService, type CreateVendorDto, type UpdateVendorDto, type VendorDto } from "../services";
import { VENDOR_STATUS_OPTIONS, VENDOR_TYPE_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const VendorForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [vendor, setVendor] = useState<VendorDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadVendor(id);
    }
  }, [isEdit, id]);

  const loadVendor = async (vendorId: string) => {
    try {
      const vendorData = await vendorService.getById(vendorId);
      setVendor(vendorData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load vendor");
    }
  };

  const vendorSchema = yup.object({
    vendorCode: yup.string().required("Vendor code is required"),
    vendorName: yup.string().required("Vendor name is required"),
    vendorType: yup.string(),
    phone: yup.string(),
    phoneNumber: yup.string(),
    email: yup.string().email("Invalid email"),
    address: yup.string(),
    addressLine1: yup.string(),
    addressLine2: yup.string(),
    city: yup.string(),
    state: yup.string(),
    country: yup.string(),
    postalCode: yup.string(),
    taxNumber: yup.string(),
    website: yup.string().url("Invalid website URL"),
    paymentTerms: yup.string(),
    creditLimit: yup.number().min(0, "Credit limit must be >= 0"),
    status: yup.string(),
    category: yup.string(),
    notes: yup.string(),
    contactPerson: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateVendorDto = {
          id,
          vendorCode: data.vendorCode,
          vendorName: data.vendorName,
          vendorType: data.vendorType || undefined,
          phone: data.phone || data.phoneNumber || undefined,
          phoneNumber: data.phoneNumber || data.phone || undefined,
          address: data.address || data.addressLine1 || undefined,
          addressLine1: data.addressLine1 || data.address || undefined,
          addressLine2: data.addressLine2 || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          postalCode: data.postalCode || undefined,
          contactPerson: data.contactPerson || undefined,
          email: data.email || undefined,
          taxNumber: data.taxNumber || undefined,
          website: data.website || undefined,
          paymentTerms: data.paymentTerms || undefined,
          creditLimit: data.creditLimit ? parseFloat(data.creditLimit) : undefined,
          status: data.status || "Active",
          category: data.category || undefined,
          notes: data.notes || undefined,
          isActive: data.isActive ?? true,
        };
        await vendorService.update(id, updateData);
      } else {
        const createData: CreateVendorDto = {
          vendorCode: data.vendorCode,
          vendorName: data.vendorName,
          vendorType: data.vendorType || undefined,
          phone: data.phone || data.phoneNumber || undefined,
          phoneNumber: data.phoneNumber || data.phone || undefined,
          address: data.address || data.addressLine1 || undefined,
          addressLine1: data.addressLine1 || data.address || undefined,
          addressLine2: data.addressLine2 || undefined,
          city: data.city || undefined,
          state: data.state || undefined,
          country: data.country || undefined,
          postalCode: data.postalCode || undefined,
          contactPerson: data.contactPerson || undefined,
          email: data.email || undefined,
          taxNumber: data.taxNumber || undefined,
          website: data.website || undefined,
          paymentTerms: data.paymentTerms || undefined,
          creditLimit: data.creditLimit ? parseFloat(data.creditLimit) : undefined,
          status: data.status || "Active",
          category: data.category || undefined,
          notes: data.notes || undefined,
          companyId: data.companyId || "", // TODO: Get from context/state
          isActive: data.isActive ?? true,
        };
        await vendorService.create(createData);
      }
      navigate("/procurement/vendors");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save vendor");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && vendor
    ? {
        vendorCode: vendor.vendorCode || "",
        vendorName: vendor.vendorName || "",
        vendorType: vendor.vendorType || "",
        phone: vendor.phone || vendor.phoneNumber || "",
        phoneNumber: vendor.phoneNumber || vendor.phone || "",
        address: vendor.address || vendor.addressLine1 || "",
        addressLine1: vendor.addressLine1 || vendor.address || "",
        addressLine2: vendor.addressLine2 || "",
        city: vendor.city || "",
        state: vendor.state || "",
        country: vendor.country || "",
        postalCode: vendor.postalCode || "",
        contactPerson: vendor.contactPerson || "",
        email: vendor.email || "",
        taxNumber: vendor.taxNumber || "",
        website: vendor.website || "",
        paymentTerms: vendor.paymentTerms || "",
        creditLimit: vendor.creditLimit || 0,
        status: vendor.status || "Active",
        category: vendor.category || "",
        notes: vendor.notes || "",
        isActive: vendor.isActive ?? true,
      }
    : {
        vendorCode: "",
        vendorName: "",
        vendorType: "",
        phone: "",
        phoneNumber: "",
        address: "",
        addressLine1: "",
        addressLine2: "",
        city: "",
        state: "",
        country: "",
        postalCode: "",
        contactPerson: "",
        email: "",
        taxNumber: "",
        website: "",
        paymentTerms: "",
        creditLimit: 0,
        status: "Active",
        category: "",
        notes: "",
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Vendor" : "Add Vendor"} subName="Procurement" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/procurement/vendors" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(vendorSchema)}
            defaultValues={defaultValues}
          >
            {/* Basic Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Basic Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Vendor Code"
                  type="text"
                  name="vendorCode"
                  placeholder="Enter vendor code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Vendor Name"
                  type="text"
                  name="vendorName"
                  placeholder="Enter vendor name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Vendor Type"
                  type="select"
                  name="vendorType"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select vendor type</option>
                  {VENDOR_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
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
                >
                  {VENDOR_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
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

                <FormInput
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="Enter email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Phone"
                  type="text"
                  name="phone"
                  placeholder="Enter phone number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Category"
                  type="text"
                  name="category"
                  placeholder="Enter category"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
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
                />

                <FormInput
                  label="State"
                  type="text"
                  name="state"
                  placeholder="Enter state"
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
                  label="Postal Code"
                  type="text"
                  name="postalCode"
                  placeholder="Enter postal code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            {/* Financial Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Financial Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Tax Number"
                  type="text"
                  name="taxNumber"
                  placeholder="Enter tax number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Website"
                  type="url"
                  name="website"
                  placeholder="Enter website URL"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Payment Terms"
                  type="text"
                  name="paymentTerms"
                  placeholder="Enter payment terms"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Credit Limit"
                  type="number"
                  name="creditLimit"
                  placeholder="0.00"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />

                <div className="md:col-span-2">
                  <FormInput
                    label="Notes"
                    type="textarea"
                    name="notes"
                    rows={4}
                    placeholder="Additional notes"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/procurement/vendors" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Vendor"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default VendorForm;

