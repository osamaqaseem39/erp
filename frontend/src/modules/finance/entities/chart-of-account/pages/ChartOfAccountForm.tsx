import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { chartOfAccountService, type CreateChartOfAccountDto, type UpdateChartOfAccountDto, type ChartOfAccountDto } from "../services";
import { ACCOUNT_TYPE_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ChartOfAccountForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const accountSchema = yup.object({
    accountCode: yup.string().required("Account code is required"),
    accountName: yup.string().required("Account name is required"),
    accountType: yup.string().required("Account type is required"),
    parentAccountId: yup.string().uuid("Invalid parent account"),
    currencyCode: yup.string(),
    isActive: yup.boolean(),
    isPostingAllowed: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [account, setAccount] = useState<ChartOfAccountDto | null>(null);
  const [parentAccounts, setParentAccounts] = useState<ChartOfAccountDto[]>([]);

  useEffect(() => {
    loadParentAccounts();
    if (isEdit && id) {
      loadAccount(id);
    }
  }, [isEdit, id]);

  const loadAccount = async (accountId: string) => {
    try {
      const accountData = await chartOfAccountService.getById(accountId);
      setAccount(accountData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load account");
    }
  };

  const loadParentAccounts = async () => {
    try {
      const accounts = await chartOfAccountService.getAll({ isActive: true });
      // Filter out the current account if editing
      if (isEdit && id) {
        setParentAccounts(accounts.filter((acc) => acc.accountId !== id));
      } else {
        setParentAccounts(accounts);
      }
    } catch (err: any) {
      console.error("Failed to load parent accounts:", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateChartOfAccountDto = {
          id,
          accountId: account?.accountId,
          accountCode: data.accountCode,
          accountName: data.accountName,
          accountType: data.accountType,
          parentAccountId: data.parentAccountId || undefined,
          currencyCode: data.currencyCode || undefined,
          isActive: data.isActive ?? true,
          isPostingAllowed: data.isPostingAllowed ?? true,
        };
        await chartOfAccountService.update(id, updateData);
      } else {
        const createData: CreateChartOfAccountDto = {
          accountCode: data.accountCode,
          accountName: data.accountName,
          accountType: data.accountType,
          parentAccountId: data.parentAccountId || undefined,
          currencyCode: data.currencyCode || undefined,
          isActive: data.isActive ?? true,
          isPostingAllowed: data.isPostingAllowed ?? true,
        };
        await chartOfAccountService.create(createData);
      }
      navigate("/finance/chart-of-accounts");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save account");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && account
    ? {
        accountCode: account.accountCode || "",
        accountName: account.accountName || "",
        accountType: account.accountType || "",
        parentAccountId: account.parentAccountId || "",
        currencyCode: account.currencyCode || "",
        isActive: account.isActive ?? true,
        isPostingAllowed: account.isPostingAllowed ?? true,
      }
    : {
        accountCode: "",
        accountName: "",
        accountType: "",
        parentAccountId: "",
        currencyCode: "",
        isActive: true,
        isPostingAllowed: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Chart of Account" : "Add Chart of Account"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/chart-of-accounts" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(accountSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Account Code"
                type="text"
                name="accountCode"
                placeholder="Enter account code"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Account Name"
                type="text"
                name="accountName"
                placeholder="Enter account name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Account Type"
                type="select"
                name="accountType"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select account type</option>
                {ACCOUNT_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Parent Account"
                type="select"
                name="parentAccountId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">None (Root Account)</option>
                {parentAccounts.map((parent) => (
                  <option key={parent.accountId || parent.id} value={parent.accountId || parent.id}>
                    {parent.accountCode} - {parent.accountName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Currency Code"
                type="text"
                name="currencyCode"
                placeholder="USD, EUR, etc."
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <div className="flex items-center gap-6">
                <FormInput
                  label="Is Active"
                  type="checkbox"
                  name="isActive"
                  containerClass="mb-4"
                  className="form-checkbox"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Posting Allowed"
                  type="checkbox"
                  name="isPostingAllowed"
                  containerClass="mb-4"
                  className="form-checkbox"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/chart-of-accounts" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Chart Of Account"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ChartOfAccountForm;

