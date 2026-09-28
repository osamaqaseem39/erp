import React, { useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../components";
import { VerticalForm, FormInput } from "../../../../components";
// Use Core module service to avoid duplication
import { userService } from "../../../../modules/core/entities/user/services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ResetPassword: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const passwordSchema = yup.object({
    newPassword: yup.string().min(6, "Password must be at least 6 characters").required("Password is required"),
    confirmPassword: yup.string()
      .oneOf([yup.ref("newPassword")], "Passwords must match")
      .required("Please confirm password"),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const onSubmit = async (data: any) => {
    if (!id) {
      setError("User ID is required");
      return;
    }

    setError(null);
    setLoading(true);
    try {
      await userService.resetPassword(id, {
        userId: id,
        newPassword: data.newPassword,
      });
      navigate("/erp/admin/users");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to reset password");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title="Reset Password" subName="Admin" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/erp/admin/users" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">Reset User Password</h4>
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
            resolver={yupResolver(passwordSchema)}
            defaultValues={{
              newPassword: "",
              confirmPassword: "",
            }}
          >
            <FormInput
              label="New Password"
              type="password"
              name="newPassword"
              placeholder="Enter new password"
              containerClass="mb-4"
              className="form-input"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
            />

            <FormInput
              label="Confirm Password"
              type="password"
              name="confirmPassword"
              placeholder="Confirm new password"
              containerClass="mb-4"
              className="form-input"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
            />

            <div className="flex justify-end gap-2">
              <Link to="/erp/admin/users" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn bg-primary text-white" disabled={loading}>
                {loading ? "Resetting..." : "Reset Password"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ResetPassword;

