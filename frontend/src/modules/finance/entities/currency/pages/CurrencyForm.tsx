import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { currencyService, type CreateCurrencyDto, type UpdateCurrencyDto, type CurrencyDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const CurrencyForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const currencySchema = yup.object({
    currencyCode: yup.string().required("Currency code is required"),
    currencyName: yup.string().required("Currency name is required"),
    iso4217: yup.string().required("ISO 4217 code is required"),
    symbol: yup.string().required("Symbol is required"),
    decimalPlaces: yup.number().min(0).max(4).required("Decimal places is required"),
    isBaseCurrency: yup.boolean(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [currency, setCurrency] = useState<CurrencyDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadCurrency(id);
    }
  }, [isEdit, id]);

  const loadCurrency = async (currencyCode: string) => {
    try {
      const currencyData = await currencyService.getById(currencyCode);
      setCurrency(currencyData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load currency");
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateCurrencyDto = {
          currencyCode: data.currencyCode,
          currencyName: data.currencyName,
          iso4217: data.iso4217,
          symbol: data.symbol,
          decimalPlaces: parseInt(data.decimalPlaces) || 2,
          isBaseCurrency: data.isBaseCurrency ?? false,
          isActive: data.isActive ?? true,
        };
        await currencyService.update(id, updateData);
      } else {
        const createData: CreateCurrencyDto = {
          currencyCode: data.currencyCode,
          currencyName: data.currencyName,
          iso4217: data.iso4217,
          symbol: data.symbol,
          decimalPlaces: parseInt(data.decimalPlaces) || 2,
          isBaseCurrency: data.isBaseCurrency ?? false,
          isActive: data.isActive ?? true,
        };
        await currencyService.create(createData);
      }
      navigate("/finance/currencies");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save currency");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && currency
    ? {
        currencyCode: currency.currencyCode || "",
        currencyName: currency.currencyName || "",
        iso4217: currency.iso4217 || "",
        symbol: currency.symbol || "",
        decimalPlaces: currency.decimalPlaces ?? 2,
        isBaseCurrency: currency.isBaseCurrency ?? false,
        isActive: currency.isActive ?? true,
      }
    : {
        currencyCode: "",
        currencyName: "",
        iso4217: "",
        symbol: "",
        decimalPlaces: 2,
        isBaseCurrency: false,
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Currency" : "Add Currency"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/currencies" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(currencySchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Currency Code"
                type="text"
                name="currencyCode"
                placeholder="USD, EUR, GBP, etc."
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
                disabled={isEdit}
              />

              <FormInput
                label="Currency Name"
                type="text"
                name="currencyName"
                placeholder="US Dollar, Euro, etc."
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="ISO 4217 Code"
                type="text"
                name="iso4217"
                placeholder="840, 978, etc."
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Symbol"
                type="text"
                name="symbol"
                placeholder="$, €, £, etc."
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Decimal Places"
                type="number"
                name="decimalPlaces"
                placeholder="2"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                min="0"
                max="4"
                required
              />

              <div className="flex items-center gap-6">
                <FormInput
                  label="Is Base Currency"
                  type="checkbox"
                  name="isBaseCurrency"
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

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/currencies" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving�" : isEdit ? "Save changes" : "Add Currency"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default CurrencyForm;

