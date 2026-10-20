import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { FormInput, PageBreadcrumb, VerticalForm } from "../../../../../components";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { ADMIN_EMAIL_TYPES, adminEmailService, type AdminEmailDto } from "../services/adminEmailService";

const schema = yup.object({
  email: yup.string().email("Invalid email").required("Email is required"),
  isActive: yup.boolean(),
});

const AdminEmailForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [row, setRow] = useState<AdminEmailDto | null>(null);
  const [type, setType] = useState<string>("Notification");

  useEffect(() => {
    if (!isEdit || !id) return;
    adminEmailService
      .getById(id)
      .then((data) => {
        setRow(data);
        setType(data.type || "Notification");
      })
      .catch((err: any) => setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load"));
  }, [isEdit, id]);

  const onSubmit = async (data: any) => {
    setError(null);
    if (!type) {
      setError("Type is required");
      return;
    }
    setLoading(true);
    try {
      if (isEdit && id) {
        await adminEmailService.update(id, {
          email: data.email,
          type,
          isActive: data.isActive ?? true,
        });
      } else {
        await adminEmailService.create({ email: data.email, type });
      }
      navigate("/core/settings/admin-emails");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit admin email" : "Add admin email"} subName="Core" />
      <div className="card">
        <div className="card-header flex items-center gap-2">
          <Link to="/core/settings/admin-emails" className="btn btn-sm btn-soft-secondary">
            <i className="mgc_arrow_left_line me-2" />
            Back
          </Link>
          <h4 className="card-title">{isEdit ? "Edit admin email" : "Add admin email"}</h4>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          {(isEdit ? row : true) && (
            <VerticalForm
              onSubmit={onSubmit}
              resolver={yupResolver(schema)}
              defaultValues={
                row || {
                  email: "",
                  isActive: true,
                }
              }
            >
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <FormInput
                  label="Email"
                  type="email"
                  name="email"
                  placeholder="ops@company.com"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium mb-2"
                  required
                />
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-2" htmlFor="admin-email-type">
                    Type <span className="text-danger">*</span>
                  </label>
                  <select
                    id="admin-email-type"
                    className="form-select"
                    value={type}
                    onChange={(e) => setType(e.target.value)}
                  >
                    {ADMIN_EMAIL_TYPES.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                  <small className="text-gray-500">Used to route notifications (Finance, Support, …).</small>
                </div>
              </div>
              {isEdit && (
                <FormInput
                  label="Active"
                  type="checkbox"
                  name="isActive"
                  containerClass="flex items-center mb-4"
                  labelClassName="ms-2"
                  className="form-checkbox rounded"
                />
              )}
              <div className="flex justify-end gap-2">
                <Link to="/core/settings/admin-emails" className="btn btn-soft-secondary">
                  Cancel
                </Link>
                <button type="submit" className="btn btn-primary" disabled={loading}>
                  {loading ? "Saving…" : isEdit ? "Save changes" : "Add email"}
                </button>
              </div>
            </VerticalForm>
          )}
        </div>
      </div>
    </>
  );
};

export default AdminEmailForm;
