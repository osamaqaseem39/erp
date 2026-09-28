import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { systemSettingService, type CreateSystemSettingDto, type UpdateSystemSettingDto, type SystemSettingDto } from "../services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const SystemSettingForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const settingSchema = yup.object({
    settingKey: yup.string().required("Setting key is required"),
    settingValue: yup.string().required("Setting value is required"),
    description: yup.string(),
    settingType: yup.string().required("Setting type is required"),
    category: yup.string().required("Category is required"),
    isEncrypted: yup.boolean(),
    isReadOnly: yup.boolean(),
    validationRule: yup.string(),
    notes: yup.string(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [setting, setSetting] = useState<SystemSettingDto | null>(null);

  useEffect(() => {
    if (isEdit && id) {
      loadSetting(id);
    }
  }, [isEdit, id]);

  const loadSetting = async (settingId: string) => {
    try {
      const settingData = await systemSettingService.getById(settingId);
      setSetting(settingData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load setting");
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateSystemSettingDto = {
          id,
          settingValue: data.settingValue,
          description: data.description,
          settingType: data.settingType,
          isEncrypted: data.isEncrypted,
          validationRule: data.validationRule,
          notes: data.notes,
        };
        await systemSettingService.update(id, updateData);
      } else {
        const createData: CreateSystemSettingDto = {
          settingKey: data.settingKey,
          settingValue: data.settingValue,
          description: data.description,
          settingType: data.settingType,
          category: data.category,
          isEncrypted: data.isEncrypted ?? false,
          isReadOnly: data.isReadOnly ?? false,
          validationRule: data.validationRule,
          notes: data.notes,
        };
        await systemSettingService.create(createData);
      }
      navigate("/core/settings/system-settings");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add System Setting"} setting`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit System Setting" : "Add System Setting"} subName="Core" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/core/settings/system-settings" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit System Setting" : "Add System Setting"}</h4>
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
            resolver={yupResolver(settingSchema)}
            defaultValues={setting || {
              settingKey: "",
              settingValue: "",
              description: "",
              settingType: "String",
              category: "",
              isEncrypted: false,
              isReadOnly: false,
              validationRule: "",
              notes: "",
            }}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              {!isEdit && (
                <FormInput
                  label="Setting Key"
                  type="text"
                  name="settingKey"
                  placeholder="Enter setting key"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />
              )}

              <FormInput
                label="Setting Value"
                type="text"
                name="settingValue"
                placeholder="Enter setting value"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Setting Type"
                type="select"
                name="settingType"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="String">String</option>
                <option value="Number">Number</option>
                <option value="Boolean">Boolean</option>
                <option value="JSON">JSON</option>
                <option value="Date">Date</option>
              </FormInput>

              <FormInput
                label="Category"
                type="text"
                name="category"
                placeholder="Enter category"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Description"
                type="textarea"
                name="description"
                placeholder="Enter description"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                rows={3}
              />

              <FormInput
                label="Validation Rule"
                type="text"
                name="validationRule"
                placeholder="Enter validation rule"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Notes"
                type="textarea"
                name="notes"
                placeholder="Enter notes"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                rows={3}
              />
            </div>

            <div className="flex items-center gap-4 mb-6">
              {!isEdit && (
                <FormInput
                  label="Read Only"
                  type="checkbox"
                  name="isReadOnly"
                  containerClass="flex items-center"
                  labelClassName="ms-2"
                  className="form-checkbox rounded"
                />
              )}
              <FormInput
                label="Encrypted"
                type="checkbox"
                name="isEncrypted"
                containerClass="flex items-center"
                labelClassName="ms-2"
                className="form-checkbox rounded"
              />
            </div>

            <div className="flex justify-end gap-2">
              <Link to="/core/settings/system-settings" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add System Setting"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default SystemSettingForm;

