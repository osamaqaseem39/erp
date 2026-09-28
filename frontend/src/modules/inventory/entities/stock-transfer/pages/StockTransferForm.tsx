import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { stockTransferService, type CreateStockTransferDto, type UpdateStockTransferDto, type StockTransferDto, type CreateStockTransferDetailDto } from "../services";
import { warehouseService } from "../../warehouse/services";
import { itemService } from "../../item/services";
import { STOCK_TRANSFER_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";

const StockTransferForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [transfer, setTransfer] = useState<StockTransferDto | null>(null);
  const [lines, setLines] = useState<CreateStockTransferDetailDto[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);

  useEffect(() => {
    loadWarehouses();
    loadItems();
    if (isEdit && id) {
      loadTransfer(id);
    } else {
      setLines([{
        itemId: "",
        quantity: 1,
        notes: "",
      }]);
    }
  }, [isEdit, id]);

  const loadTransfer = async (transferId: string) => {
    try {
      const transferData = await stockTransferService.getById(transferId);
      setTransfer(transferData);
      if (transferData.details && transferData.details.length > 0) {
        setLines(transferData.details.map(detail => ({
          itemId: detail.itemId,
          quantity: detail.quantity,
          notes: detail.notes || "",
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load stock transfer");
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
      quantity: 1,
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
    if (field === "quantity") {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }
    
    setLines(newLines);
  };

  const transferSchema = yup.object({
    transferNumber: yup.string(),
    transferDate: yup.string().required("Transfer date is required"),
    fromWarehouseId: yup.string().uuid("Invalid warehouse").required("From warehouse is required"),
    toWarehouseId: yup.string().uuid("Invalid warehouse").required("To warehouse is required"),
    reference: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Validate lines
    if (lines.length === 0) {
      setError("At least one transfer line is required");
      return;
    }

    // Validate from and to warehouses are different
    if (data.fromWarehouseId === data.toWarehouseId) {
      setError("From warehouse and To warehouse must be different");
      return;
    }

    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateStockTransferDto = {
          id,
          transferNumber: data.transferNumber || undefined,
          transferDate: data.transferDate,
          fromWarehouseId: data.fromWarehouseId,
          toWarehouseId: data.toWarehouseId,
          status: data.status || "Draft",
          reference: data.reference || undefined,
          notes: data.notes || undefined,
        };
        await stockTransferService.update(id, updateData);
        // TODO: Update details separately if needed
      } else {
        const createData: CreateStockTransferDto = {
          transferNumber: data.transferNumber || undefined,
          transferDate: data.transferDate,
          fromWarehouseId: data.fromWarehouseId,
          toWarehouseId: data.toWarehouseId,
          reference: data.reference || undefined,
          notes: data.notes || undefined,
          details: lines,
        };
        await stockTransferService.create(createData);
      }
      navigate("/inventory/stock-transfers");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save stock transfer");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && transfer
    ? {
        transferNumber: transfer.transferNumber || "",
        transferDate: transfer.transferDate ? transfer.transferDate.split('T')[0] : new Date().toISOString().split('T')[0],
        fromWarehouseId: transfer.fromWarehouseId || "",
        toWarehouseId: transfer.toWarehouseId || "",
        status: transfer.status || "Draft",
        reference: transfer.reference || "",
        notes: transfer.notes || "",
      }
    : {
        transferNumber: "",
        transferDate: new Date().toISOString().split('T')[0],
        fromWarehouseId: "",
        toWarehouseId: "",
        status: "Draft",
        reference: "",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Stock Transfer" : "Add Stock Transfer"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/stock-transfers" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(transferSchema)}
            defaultValues={defaultValues}
          >
            {/* Transfer Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Transfer Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Transfer Number"
                  type="text"
                  name="transferNumber"
                  placeholder="Auto-generated if empty"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Transfer Date"
                  type="date"
                  name="transferDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="From Warehouse"
                  type="select"
                  name="fromWarehouseId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select from warehouse</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.warehouseCode} - {wh.warehouseName}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="To Warehouse"
                  type="select"
                  name="toWarehouseId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select to warehouse</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.warehouseCode} - {wh.warehouseName}
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
                  {STOCK_TRANSFER_STATUS_OPTIONS.map((option) => (
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

            {/* Transfer Details */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Transfer Details</h5>
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
                      <th>Quantity</th>
                      <th>Notes</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, index) => (
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
                            value={line.quantity}
                            onChange={(e) => updateLine(index, "quantity", e.target.value)}
                            step="0.01"
                            min="0.01"
                            required
                          />
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
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/stock-transfers" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Stock Transfer"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default StockTransferForm;

