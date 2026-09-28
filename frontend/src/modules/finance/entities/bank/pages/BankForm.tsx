import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { bankService, type CreateBankDto, type UpdateBankDto, type BankDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const BankForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const bankSchema = yup.object({
    bankName: yup.string().required("Bank name is required"),
    bankCode: yup.string().required("Bank code is required"),
    swiftCode: yup.string(),
    address: yup.string(),
    city: yup.string(),
    state: yup.string(),
    country: yup.string(),
    postalCode: yup.string(),
    phone: yup.string(),
    email: yup.string().email("Invalid email"),
    website: yup.string().url("Invalid website URL"),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [bank, setBank] = useState<BankDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadBank(id);
    }
  }, [isEdit, id]);

  const loadBank = async (bankId: string) => {
    try {
      const bankData = await bankService.getById(bankId);
      setBank(bankData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load bank");
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateBankDto = {
          id,
          bankName: data.bankName,
          bankCode: data.bankCode,
          swiftCode: data.swiftCode || "",
          address: data.address || "",
          city: data.city || "",
          state: data.state || "",
          country: data.country || "",
          postalCode: data.postalCode || "",
          phone: data.phone || "",
          email: data.email || "",
          website: data.website || "",
          isActive: data.isActive ?? true,
        };
        await bankService.update(id, updateData);
      } else {
        const createData: CreateBankDto = {
          companyId: data.companyId || "", // TODO: Get from context/state
          bankName: data.bankName,
          bankCode: data.bankCode,
          swiftCode: data.swiftCode || "",
          address: data.address || "",
          city: data.city || "",
          state: data.state || "",
          country: data.country || "",
          postalCode: data.postalCode || "",
          phone: data.phone || "",
          email: data.email || "",
          website: data.website || "",
          isActive: data.isActive ?? true,
        };
        await bankService.create(createData);
      }
      navigate("/finance/banks");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save bank");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && bank
    ? {
        bankName: bank.bankName || "",
        bankCode: bank.bankCode || "",
        swiftCode: bank.swiftCode || "",
        address: bank.address || "",
        city: bank.city || "",
        state: bank.state || "",
        country: bank.country || "",
        postalCode: bank.postalCode || "",
        phone: bank.phone || "",
        email: bank.email || "",
        website: bank.website || "",
        isActive: bank.isActive ?? true,
      }
    : {
        bankName: "",
        bankCode: "",
        swiftCode: "",
        address: "",
        city: "",
        state: "",
        country: "",
        postalCode: "",
        phone: "",
        email: "",
        website: "",
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Bank" : "Add Bank"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/banks" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(bankSchema)}
            defaultValues={defaultValues}
          >
            {/* Basic Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Basic Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Bank Name"
                  type="text"
                  name="bankName"
                  placeholder="Enter bank name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Bank Code"
                  type="text"
                  name="bankCode"
                  placeholder="Enter bank code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="SWIFT Code"
                  type="text"
                  name="swiftCode"
                  placeholder="Enter SWIFT code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <div className="mb-4">
                  <FormInput
                    label="Active"
                    type="checkbox"
                    name="isActive"
                    containerClass="flex items-center"
                    labelClassName="ms-2"
                    className="form-checkbox rounded"
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
                    label="Address"
                    type="text"
                    name="address"
                    placeholder="Enter address"
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

            {/* Contact Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Contact Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
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
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="Enter email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Website"
                  type="url"
                  name="website"
                  placeholder="https://example.com"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/banks" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Bank"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default BankForm;

