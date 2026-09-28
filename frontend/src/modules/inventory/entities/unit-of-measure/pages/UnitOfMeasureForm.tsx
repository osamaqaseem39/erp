import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { unitOfMeasureService, type CreateUnitOfMeasureDto, type UpdateUnitOfMeasureDto, type UnitOfMeasureDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const UnitOfMeasureForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const unitSchema = yup.object({
    code: yup.string().required("Unit code is required"),
    name: yup.string().required("Unit name is required"),
    symbol: yup.string().required("Symbol is required"),
    conversionFactor: yup.number().min(0.0001, "Conversion factor must be > 0").required("Conversion factor is required"),
    baseUnitId: yup.string().uuid("Invalid base unit").nullable(),
    isBaseUnit: yup.boolean(),
    isActive: yup.boolean(),
    description: yup.string(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [unit, setUnit] = useState<UnitOfMeasureDto | null>(null);
  const [baseUnits, setBaseUnits] = useState<UnitOfMeasureDto[]>([]);

  useEffect(() => {
    loadBaseUnits();
    if (isEdit && id) {
      loadUnit(id);
    }
  }, [isEdit, id]);

  const loadUnit = async (unitId: string) => {
    try {
      const unitData = await unitOfMeasureService.getById(unitId);
      setUnit(unitData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load unit");
    }
  };

  const loadBaseUnits = async () => {
    try {
      const units = await unitOfMeasureService.getAll(true); // Get active units only
      // Filter out the current unit if editing and only base units
      if (isEdit && id) {
        setBaseUnits(units.filter((u) => u.id !== id && u.isBaseUnit));
      } else {
        setBaseUnits(units.filter((u) => u.isBaseUnit));
      }
    } catch (err: any) {
      console.error("Failed to load base units:", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateUnitOfMeasureDto = {
          id,
          code: data.code,
          name: data.name,
          symbol: data.symbol,
          conversionFactor: parseFloat(data.conversionFactor) || 1,
          baseUnitId: data.baseUnitId || undefined,
          isBaseUnit: data.isBaseUnit ?? false,
          isActive: data.isActive ?? true,
          description: data.description || undefined,
        };
        await unitOfMeasureService.update(id, updateData);
      } else {
        const createData: CreateUnitOfMeasureDto = {
          code: data.code,
          name: data.name,
          symbol: data.symbol,
          conversionFactor: parseFloat(data.conversionFactor) || 1,
          baseUnitId: data.baseUnitId || undefined,
          isBaseUnit: data.isBaseUnit ?? false,
          isActive: data.isActive ?? true,
          description: data.description || undefined,
        };
        await unitOfMeasureService.create(createData);
      }
      navigate("/inventory/units-of-measure");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save unit");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && unit
    ? {
        code: unit.code || "",
        name: unit.name || "",
        symbol: unit.symbol || "",
        conversionFactor: unit.conversionFactor ?? 1,
        baseUnitId: unit.baseUnitId || "",
        isBaseUnit: unit.isBaseUnit ?? false,
        isActive: unit.isActive ?? true,
        description: unit.description || "",
      }
    : {
        code: "",
        name: "",
        symbol: "",
        conversionFactor: 1,
        baseUnitId: "",
        isBaseUnit: false,
        isActive: true,
        description: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Unit of Measure" : "Add Unit of Measure"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/units-of-measure" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(unitSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Unit Code"
                type="text"
                name="code"
                placeholder="e.g., KG, L, PCS"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Unit Name"
                type="text"
                name="name"
                placeholder="e.g., Kilogram, Liter, Pieces"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Symbol"
                type="text"
                name="symbol"
                placeholder="e.g., kg, L, pcs"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Conversion Factor"
                type="number"
                name="conversionFactor"
                placeholder="1.0"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                min="0.0001"
                step="0.0001"
                required
              />

              <FormInput
                label="Base Unit"
                type="select"
                name="baseUnitId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                disabled={defaultValues.isBaseUnit}
              >
                <option value="">None (This is a base unit)</option>
                {baseUnits.map((baseUnit) => (
                  <option key={baseUnit.id} value={baseUnit.id}>
                    {baseUnit.code} - {baseUnit.name}
                  </option>
                ))}
              </FormInput>

              <div className="flex items-center gap-6">
                <FormInput
                  label="Is Base Unit"
                  type="checkbox"
                  name="isBaseUnit"
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

              <div className="md:col-span-2">
                <FormInput
                  label="Description"
                  type="textarea"
                  name="description"
                  rows={3}
                  placeholder="Enter unit description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/units-of-measure" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Unit Of Measure"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default UnitOfMeasureForm;

