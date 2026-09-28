import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { itemService, type CreateItemDto, type UpdateItemDto, type ItemDto } from "../services";
import { itemCategoryService } from "../../item-category/services";
import { unitOfMeasureService } from "../../unit-of-measure/services";
import { warehouseService } from "../../warehouse/services";
import { ITEM_TYPE_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ItemForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const itemSchema = yup.object({
    itemCode: yup.string().required("Item code is required"),
    itemName: yup.string().required("Item name is required"),
    description: yup.string(),
    itemCategoryId: yup.string().uuid("Invalid category").required("Category is required"),
    categoryId: yup.string().required("Category ID is required"),
    itemType: yup.string().required("Item type is required"),
    unitOfMeasureId: yup.string().uuid("Invalid unit of measure").required("Unit of measure is required"),
    hsnCode: yup.string(),
    barCode: yup.string(),
    defaultWarehouseId: yup.string().uuid("Invalid warehouse").nullable(),
    costPrice: yup.number().min(0, "Cost price must be >= 0").required("Cost price is required"),
    sellingPrice: yup.number().min(0, "Selling price must be >= 0").required("Selling price is required"),
    minimumStock: yup.number().min(0, "Minimum stock must be >= 0"),
    maximumStock: yup.number().min(0, "Maximum stock must be >= 0"),
    reorderLevel: yup.number().min(0, "Reorder level must be >= 0"),
    reorderQuantity: yup.number().min(0, "Reorder quantity must be >= 0"),
    isSerialized: yup.boolean(),
    isBatchTracked: yup.boolean(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [item, setItem] = useState<ItemDto | null>(null);
  const [categories, setCategories] = useState<any[]>([]);
  const [unitsOfMeasure, setUnitsOfMeasure] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]);

  useEffect(() => {
    loadCategories();
    loadUnitsOfMeasure();
    loadWarehouses();
    if (isEdit && id) {
      loadItem(id);
    }
  }, [isEdit, id]);

  const loadItem = async (itemId: string) => {
    try {
      const itemData = await itemService.getById(itemId);
      setItem(itemData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load item");
    }
  };

  const loadCategories = async () => {
    try {
      const data = await itemCategoryService.getAll();
      setCategories(data);
    } catch (err: any) {
      console.error("Failed to load categories", err);
    }
  };

  const loadUnitsOfMeasure = async () => {
    try {
      const data = await unitOfMeasureService.getAll(true);
      setUnitsOfMeasure(data);
    } catch (err: any) {
      console.error("Failed to load units of measure", err);
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

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateItemDto = {
          id,
          itemCode: data.itemCode,
          itemName: data.itemName,
          description: data.description || "",
          itemCategoryId: data.itemCategoryId,
          categoryId: data.categoryId || data.itemCategoryId,
          itemType: data.itemType,
          unitOfMeasureId: data.unitOfMeasureId,
          hsnCode: data.hsnCode || "",
          barCode: data.barCode || "",
          defaultWarehouseId: data.defaultWarehouseId || undefined,
          costPrice: parseFloat(data.costPrice) || 0,
          sellingPrice: parseFloat(data.sellingPrice) || 0,
          minimumStock: parseFloat(data.minimumStock) || 0,
          maximumStock: parseFloat(data.maximumStock) || 0,
          reorderLevel: parseFloat(data.reorderLevel) || 0,
          reorderQuantity: parseFloat(data.reorderQuantity) || 0,
          isSerialized: data.isSerialized ?? false,
          isBatchTracked: data.isBatchTracked ?? false,
          isActive: data.isActive ?? true,
        };
        await itemService.update(id, updateData);
      } else {
        const createData: CreateItemDto = {
          companyId: data.companyId || "", // TODO: Get from context/state
          itemCode: data.itemCode,
          itemName: data.itemName,
          description: data.description || "",
          itemCategoryId: data.itemCategoryId,
          categoryId: data.categoryId || data.itemCategoryId,
          itemType: data.itemType,
          unitOfMeasureId: data.unitOfMeasureId,
          hsnCode: data.hsnCode || "",
          barCode: data.barCode || "",
          defaultWarehouseId: data.defaultWarehouseId || undefined,
          costPrice: parseFloat(data.costPrice) || 0,
          sellingPrice: parseFloat(data.sellingPrice) || 0,
          minimumStock: parseFloat(data.minimumStock) || 0,
          maximumStock: parseFloat(data.maximumStock) || 0,
          reorderLevel: parseFloat(data.reorderLevel) || 0,
          reorderQuantity: parseFloat(data.reorderQuantity) || 0,
          isSerialized: data.isSerialized ?? false,
          isBatchTracked: data.isBatchTracked ?? false,
          isActive: data.isActive ?? true,
        };
        await itemService.create(createData);
      }
      navigate("/inventory/items");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save item");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && item
    ? {
        itemCode: item.itemCode || "",
        itemName: item.itemName || "",
        description: item.description || "",
        itemCategoryId: item.itemCategoryId || "",
        categoryId: item.categoryId || item.itemCategoryId || "",
        itemType: item.itemType || "",
        unitOfMeasureId: item.unitOfMeasureId || "",
        hsnCode: item.hsnCode || "",
        barCode: item.barCode || "",
        defaultWarehouseId: item.defaultWarehouseId || "",
        costPrice: item.costPrice ?? 0,
        sellingPrice: item.sellingPrice ?? 0,
        minimumStock: item.minimumStock ?? 0,
        maximumStock: item.maximumStock ?? 0,
        reorderLevel: item.reorderLevel ?? 0,
        reorderQuantity: item.reorderQuantity ?? 0,
        isSerialized: item.isSerialized ?? false,
        isBatchTracked: item.isBatchTracked ?? false,
        isActive: item.isActive ?? true,
      }
    : {
        itemCode: "",
        itemName: "",
        description: "",
        itemCategoryId: "",
        categoryId: "",
        itemType: "",
        unitOfMeasureId: "",
        hsnCode: "",
        barCode: "",
        defaultWarehouseId: "",
        costPrice: 0,
        sellingPrice: 0,
        minimumStock: 0,
        maximumStock: 0,
        reorderLevel: 0,
        reorderQuantity: 0,
        isSerialized: false,
        isBatchTracked: false,
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Item" : "Add Item"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/items" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(itemSchema)}
            defaultValues={defaultValues}
          >
            {/* Basic Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Basic Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Item Code"
                  type="text"
                  name="itemCode"
                  placeholder="Enter item code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Item Name"
                  type="text"
                  name="itemName"
                  placeholder="Enter item name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Item Category"
                  type="select"
                  name="itemCategoryId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                  onChange={(e: any) => {
                    // Set categoryId to same value for compatibility
                    const formContext = e.target.form;
                    if (formContext) {
                      const categoryIdInput = formContext.querySelector('[name="categoryId"]');
                      if (categoryIdInput) {
                        categoryIdInput.value = e.target.value;
                      }
                    }
                  }}
                >
                  <option value="">Select category</option>
                  {categories.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.code} - {cat.name}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Item Type"
                  type="select"
                  name="itemType"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select item type</option>
                  {ITEM_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Unit of Measure"
                  type="select"
                  name="unitOfMeasureId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select unit of measure</option>
                  {unitsOfMeasure.map((uom) => (
                    <option key={uom.id} value={uom.id}>
                      {uom.code} - {uom.name}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Default Warehouse"
                  type="select"
                  name="defaultWarehouseId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select warehouse</option>
                  {warehouses.map((wh) => (
                    <option key={wh.id} value={wh.id}>
                      {wh.warehouseCode} - {wh.warehouseName}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="HSN Code"
                  type="text"
                  name="hsnCode"
                  placeholder="Enter HSN code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Bar Code"
                  type="text"
                  name="barCode"
                  placeholder="Enter bar code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <div className="md:col-span-2">
                  <FormInput
                    label="Description"
                    type="textarea"
                    name="description"
                    rows={3}
                    placeholder="Enter item description"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            {/* Pricing Information */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Pricing Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Cost Price"
                  type="number"
                  name="costPrice"
                  placeholder="0.00"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                  required
                />

                <FormInput
                  label="Selling Price"
                  type="number"
                  name="sellingPrice"
                  placeholder="0.00"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                  required
                />
              </div>
            </div>

            {/* Stock Management */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Stock Management</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Minimum Stock"
                  type="number"
                  name="minimumStock"
                  placeholder="0"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />

                <FormInput
                  label="Maximum Stock"
                  type="number"
                  name="maximumStock"
                  placeholder="0"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />

                <FormInput
                  label="Reorder Level"
                  type="number"
                  name="reorderLevel"
                  placeholder="0"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />

                <FormInput
                  label="Reorder Quantity"
                  type="number"
                  name="reorderQuantity"
                  placeholder="0"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />
              </div>
            </div>

            {/* Tracking Options */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Tracking Options</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
                <FormInput
                  label="Is Serialized"
                  type="checkbox"
                  name="isSerialized"
                  containerClass="mb-4"
                  className="form-checkbox"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Is Batch Tracked"
                  type="checkbox"
                  name="isBatchTracked"
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

            {/* Hidden field for categoryId compatibility */}
            <input type="hidden" name="categoryId" value={defaultValues.categoryId} />

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/items" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Item"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ItemForm;

