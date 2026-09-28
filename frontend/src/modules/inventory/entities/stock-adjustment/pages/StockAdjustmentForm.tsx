import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { stockAdjustmentService, type CreateStockAdjustmentDto, type UpdateStockAdjustmentDto, type StockAdjustmentDto, type CreateStockAdjustmentDetailDto } from "../services";
import { warehouseService } from "../../warehouse/services";
import { itemService } from "../../item/services";
import { STOCK_ADJUSTMENT_TYPE_OPTIONS, STOCK_ADJUSTMENT_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";

const StockAdjustmentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [adjustment, setAdjustment] = useState<StockAdjustmentDto | null>(null);
  const [lines, setLines] = useState<CreateStockAdjustmentDetailDto[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);

  useEffect(() => {
    loadWarehouses();
    loadItems();
    if (isEdit && id) {
      loadAdjustment(id);
    } else {
      setLines([{
        itemId: "",
        quantityOnHand: 0,
        quantityCounted: 0,
        notes: "",
      }]);
    }
  }, [isEdit, id]);

  const loadAdjustment = async (adjustmentId: string) => {
    try {
      const adjustmentData = await stockAdjustmentService.getById(adjustmentId);
      setAdjustment(adjustmentData);
      if (adjustmentData.details && adjustmentData.details.length > 0) {
        setLines(adjustmentData.details.map(detail => ({
          itemId: detail.itemId,
          quantityOnHand: detail.quantityOnHand,
          quantityCounted: detail.quantityCounted,
          notes: detail.notes || "",
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load stock adjustment");
    }
  };

  const loadWarehouses = async () => {
    try {
      const data = await warehouseService.getActive();
      setWarehouses(data);
    } catch (err: any) {
      console.error("Failed to load warehouses", err);
    }
  };

  const loadItems = async () => {
    try {
      const companyId = getCompanyId();
      const data = await itemService.getAll({ companyId });
      setItems(data);
    } catch (err: any) {
      console.error("Failed to load items", err);
    }
  };

  const addLine = () => {
    setLines([...lines, {
      itemId: "",
      quantityOnHand: 0,
      quantityCounted: 0,
      notes: "",
    }]);
  };

  const removeLine = (index: number) => {
    const newLines = lines.filter((_, i) => i !== index);
    setLines(newLines);
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;
    
    // Ensure numeric fields are numbers
    if (field === "quantityOnHand" || field === "quantityCounted") {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
      // Calculate difference
      const quantityOnHand = field === "quantityOnHand" ? parseFloat(value) || 0 : newLines[index].quantityOnHand || 0;
      const quantityCounted = field === "quantityCounted" ? parseFloat(value) || 0 : newLines[index].quantityCounted || 0;
      // Difference is calculated automatically on backend, but we can show it
    }
    
    setLines(newLines);
  };

  const adjustmentSchema = yup.object({
    adjustmentNumber: yup.string(),
    adjustmentDate: yup.string().required("Adjustment date is required"),
    warehouseId: yup.string().uuid("Invalid warehouse").required("Warehouse is required"),
    adjustmentType: yup.string(),
    reference: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Validate lines
    if (lines.length === 0) {
      setError("At least one adjustment line is required");
      return;
    }

    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateStockAdjustmentDto = {
          id,
          adjustmentNumber: data.adjustmentNumber || undefined,
          adjustmentDate: data.adjustmentDate,
          warehouseId: data.warehouseId,
          adjustmentType: data.adjustmentType || "Physical",
          status: data.status || "Draft",
          reference: data.reference || undefined,
          notes: data.notes || undefined,
        };
        await stockAdjustmentService.update(id, updateData);
        // TODO: Update details separately if needed
      } else {
        const createData: CreateStockAdjustmentDto = {
          adjustmentNumber: data.adjustmentNumber || undefined,
          adjustmentDate: data.adjustmentDate,
          warehouseId: data.warehouseId,
          adjustmentType: data.adjustmentType || "Physical",
          reference: data.reference || undefined,
          notes: data.notes || undefined,
          details: lines,
        };
        await stockAdjustmentService.create(createData);
      }
      navigate("/inventory/stock-adjustments");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save stock adjustment");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && adjustment
    ? {
        adjustmentNumber: adjustment.adjustmentNumber || "",
        adjustmentDate: adjustment.adjustmentDate ? adjustment.adjustmentDate.split('T')[0] : new Date().toISOString().split('T')[0],
        warehouseId: adjustment.warehouseId || "",
        adjustmentType: adjustment.adjustmentType || "Physical",
        status: adjustment.status || "Draft",
        reference: adjustment.reference || "",
        notes: adjustment.notes || "",
      }
    : {
        adjustmentNumber: "",
        adjustmentDate: new Date().toISOString().split('T')[0],
        warehouseId: "",
        adjustmentType: "Physical",
        status: "Draft",
        reference: "",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Stock Adjustment" : "Add Stock Adjustment"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/stock-adjustments" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(adjustmentSchema)}
            defaultValues={defaultValues}
          >
            {/* Adjustment Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Adjustment Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Adjustment Number"
                  type="text"
                  name="adjustmentNumber"
                  placeholder="Auto-generated if empty"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Adjustment Date"
                  type="date"
                  name="adjustmentDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Warehouse"
                  type="select"
                  name="warehouseId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select warehouse</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.warehouseCode} - {wh.warehouseName}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Adjustment Type"
                  type="select"
                  name="adjustmentType"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  {STOCK_ADJUSTMENT_TYPE_OPTIONS.map((option) => (
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
                  {STOCK_ADJUSTMENT_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Reference"
                  type="text"
                  name="reference"
                  placeholder="Reference number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <div className="md:col-span-2">
                  <FormInput
                    label="Notes"
                    type="textarea"
                    name="notes"
                    rows={3}
                    placeholder="Additional notes"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            {/* Adjustment Details */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Adjustment Details</h5>
                <button
                  type="button"
                  onClick={addLine}
                  className="btn btn-sm btn-primary"
                >
                  <i className="mgc_add_line me-2"></i>Add Line
                </button>
              </div>

              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Item</th>
                      <th>Quantity On Hand</th>
                      <th>Quantity Counted</th>
                      <th>Difference</th>
                      <th>Notes</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => {
                      const difference = (line.quantityCounted || 0) - (line.quantityOnHand || 0);
                      return (
                        <tr key={index}>
                          <td>
                            <select
                              className="form-select form-select-sm"
                              value={line.itemId}
                              onChange={(e) => updateLine(index, "itemId", e.target.value)}
                              required
                            >
                              <option value="">Select item</option>
                              {items.map((item) => (
                                <option key={item.id} value={item.id}>
                                  {item.itemCode} - {item.itemName}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              value={line.quantityOnHand}
                              onChange={(e) => updateLine(index, "quantityOnHand", e.target.value)}
                              step="0.01"
                              min="0"
                              required
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              value={line.quantityCounted}
                              onChange={(e) => updateLine(index, "quantityCounted", e.target.value)}
                              step="0.01"
                              min="0"
                              required
                            />
                          </td>
                          <td>
                            <span className={difference >= 0 ? "text-success" : "text-danger"}>
                              {difference >= 0 ? "+" : ""}{difference.toFixed(2)}
                            </span>
                          </td>
                          <td>
                            <input
                              type="text"
                              className="form-control form-control-sm"
                              value={line.notes || ""}
                              onChange={(e) => updateLine(index, "notes", e.target.value)}
                              placeholder="Notes"
                            />
                          </td>
                          <td>
                            {lines.length > 1 && (
                              <button
                                type="button"
                                onClick={() => removeLine(index)}
                                className="btn btn-sm btn-soft-danger"
                              >
                                <i className="mgc_delete_line"></i>
                              </button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/stock-adjustments" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Stock Adjustment"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default StockAdjustmentForm;

