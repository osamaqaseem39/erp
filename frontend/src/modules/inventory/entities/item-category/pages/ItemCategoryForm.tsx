import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { itemCategoryService, type CreateItemCategoryDto, type UpdateItemCategoryDto, type ItemCategoryDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ItemCategoryForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const categorySchema = yup.object({
    code: yup.string().required("Category code is required"),
    name: yup.string().required("Category name is required"),
    description: yup.string(),
    parentCategoryId: yup.string().uuid("Invalid parent category").nullable(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [category, setCategory] = useState<ItemCategoryDto | null>(null);
  const [parentCategories, setParentCategories] = useState<ItemCategoryDto[]>([]);

  useEffect(() => {
    loadParentCategories();
    if (isEdit && id) {
      loadCategory(id);
    }
  }, [isEdit, id]);

  const loadCategory = async (categoryId: string) => {
    try {
      const categoryData = await itemCategoryService.getById(categoryId);
      setCategory(categoryData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load category");
    }
  };

  const loadParentCategories = async () => {
    try {
      const categories = await itemCategoryService.getAll();
      // Filter out the current category if editing
      if (isEdit && id) {
        setParentCategories(categories.filter((cat) => cat.id !== id));
      } else {
        setParentCategories(categories);
      }
    } catch (err: any) {
      console.error("Failed to load parent categories:", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateItemCategoryDto = {
          id,
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          parentCategoryId: data.parentCategoryId || undefined,
          isActive: data.isActive ?? true,
        };
        await itemCategoryService.update(id, updateData);
      } else {
        const createData: CreateItemCategoryDto = {
          code: data.code,
          name: data.name,
          description: data.description || undefined,
          parentCategoryId: data.parentCategoryId || undefined,
          isActive: data.isActive ?? true,
        };
        await itemCategoryService.create(createData);
      }
      navigate("/inventory/item-categories");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save category");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && category
    ? {
        code: category.code || "",
        name: category.name || "",
        description: category.description || "",
        parentCategoryId: category.parentCategoryId || "",
        isActive: category.isActive ?? true,
      }
    : {
        code: "",
        name: "",
        description: "",
        parentCategoryId: "",
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Item Category" : "Add Item Category"} subName="Inventory" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/inventory/item-categories" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(categorySchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Category Code"
                type="text"
                name="code"
                placeholder="Enter category code"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Category Name"
                type="text"
                name="name"
                placeholder="Enter category name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Parent Category"
                type="select"
                name="parentCategoryId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">None (Root Category)</option>
                {parentCategories.map((parent) => (
                  <option key={parent.id} value={parent.id}>
                    {parent.code} - {parent.name}
                  </option>
                ))}
              </FormInput>

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

              <div className="md:col-span-2">
                <FormInput
                  label="Description"
                  type="textarea"
                  name="description"
                  rows={4}
                  placeholder="Enter category description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/inventory/item-categories" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Item Category"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ItemCategoryForm;

