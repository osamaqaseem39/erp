import { safeRedirectPath } from "../../lib/erp/safeRedirect";
import { useEffect, useMemo, useState } from "react";
import { Navigate, Link, useLocation, useSearchParams } from "react-router-dom";

// form validation
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

// redux
import { useDispatch, useSelector } from "react-redux";
import { AppDispatch, RootState } from "../../redux/store";
import { loginUser, resetAuth } from "../../redux/actions";

// components
import { VerticalForm, FormInput, AuthLayout, PageBreadcrumb } from "../../components";
import { APICore } from "../../helpers/api/apiCore";
import { authService } from "../../services/erp/authService";
// Use Core module service to avoid duplication
import { tenantService, type TenantDto } from "../../modules/core/entities/tenant/services";
import type { LoginDto } from "../../types/erp";
import { clearSetupCredentials, getSetupCredentials } from "../../modules/core/constants/setupKeys";

interface UserData {
  primaryEmail: string;
  password: string;
  tenantId: string;
}

/* bottom links */
const BottomLink = () => {
  return (
    <p className="text-gray-500 dark:text-gray-400 text-center">Don't have an account ?
      <Link to="/auth/register" className="text-primary ms-1">
        <b>
          Register
        </b>
      </Link>
    </p>
  );
};

const api = new APICore();

const Login = () => {
  const dispatch = useDispatch<AppDispatch>();
  const [searchParams] = useSearchParams();
  const setupCreds = getSetupCredentials();
  const [tenants, setTenants] = useState<TenantDto[]>([]);
  const [showTenantSelect, setShowTenantSelect] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [tenantsLoaded, setTenantsLoaded] = useState(false);

  const userLoggedIn = useSelector((state: RootState) => state.Auth.userLoggedIn);
  const isAuthenticated = api.isUserAuthenticated();

  const defaultValues = useMemo<UserData>(() => ({
    primaryEmail: searchParams.get("email") || setupCreds?.email || "",
    password: "",
    tenantId: searchParams.get("tenantId") || setupCreds?.tenantId || "",
  }), [searchParams, setupCreds]);

  const fromSetup = Boolean(defaultValues.tenantId && defaultValues.primaryEmail);

  useEffect(() => {
    dispatch(resetAuth());
  }, [dispatch]);

  useEffect(() => {
    if (isAuthenticated || tenantsLoaded) {
      return;
    }

    let cancelled = false;
    tenantService.getLoginOptions()
      .then((data) => {
        if (cancelled) return;
        setTenants(data);
        setShowTenantSelect(data.length > 0);
        setTenantsLoaded(true);
      })
      .catch((err) => {
        if (cancelled) return;
        console.error("Failed to load tenants:", err);
        setShowTenantSelect(false);
        setTenantsLoaded(true);
      });

    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, tenantsLoaded]);

  /*
  form validation schema
  */
  const schemaResolver = yupResolver(
    yup.object().shape({
      primaryEmail: yup.string().email("Please enter a valid email").required("Email is required"),
      password: yup.string().min(6, "Password must be at least 6 characters").required("Password is required"),
      tenantId: yup.string().required("Tenant is required"),
    })
  );

  /*
  handle form submission
  */
  const onSubmit = async (formData: UserData) => {
    setError(null);
    setLoading(true);
    try {
      // Validate tenantId is a valid GUID format
      if (!formData.tenantId || !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(formData.tenantId)) {
        setError("Please select a valid tenant");
        setLoading(false);
        return;
      }

      const loginData: LoginDto = {
        primaryEmail: formData.primaryEmail,
        password: formData.password,
        tenantId: formData.tenantId,
      };
      
      const response = await authService.login(loginData);
      
      // Update Redux store with user data
      if (response.user) {
        dispatch(loginUser(response.user.username || response.user.email, response.user.email));
      }

      clearSetupCredentials();
      
      // Redirect will happen automatically via the Navigate component
    } catch (err: any) {
      const errorMessage = err.response?.data?.message 
        || err.response?.data?.error 
        || err.message 
        || "Login failed. Please check your credentials.";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const location = useLocation();
  const redirectUrl = safeRedirectPath(location?.search ?? "");

  return (
    <>
      {(userLoggedIn || isAuthenticated) && <Navigate to={redirectUrl} />}
      <PageBreadcrumb title="Login" />
      <AuthLayout
        authTitle="Sign In"
        helpText="Enter your email address and password to access admin panel."
        bottomLinks={<BottomLink />}
        hasThirdPartyLogin={false}
      >
        {fromSetup && (
          <div className="alert alert-success mb-4" role="alert">
            Setup complete. Sign in with your super admin credentials below.
          </div>
        )}

        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}

        <VerticalForm<UserData>
          key={`${defaultValues.tenantId}-${defaultValues.primaryEmail}`}
          onSubmit={onSubmit}
          resolver={schemaResolver}
          defaultValues={defaultValues}
        >
          {showTenantSelect && tenants.length > 0 ? (
            <FormInput
              label="Select Tenant"
              type="select"
              name="tenantId"
              containerClass="mb-4"
              className="form-select"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
            >
              <option value="">Select a tenant</option>
              {tenants.map((tenant) => (
                <option key={tenant.id} value={tenant.id}>
                  {tenant.name} ({tenant.domain})
                </option>
              ))}
            </FormInput>
          ) : (
            <FormInput
              label="Tenant ID"
              type="text"
              name="tenantId"
              placeholder="Enter tenant ID"
              containerClass="mb-4"
              className="form-input"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
            />
          )}

          <FormInput
            label="Email Address"
            type="email"
            name="primaryEmail"
            placeholder="Enter your email"
            containerClass="mb-4"
            className="form-input"
            labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
            required
          />

          <FormInput
            label="Password"
            type="password"
            name="password"
            placeholder="Enter your password"
            containerClass="mb-4"
            className="form-input"
            labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
            required
          />

          <div className="flex items-center justify-between mb-4">
            <FormInput
              label="Remember me"
              type="checkbox"
              name="checkbox"
              containerClass="flex items-center"
              labelClassName="ms-2"
              className="form-checkbox rounded"
            />
            <Link to="/auth/recover-password" className="text-sm text-primary border-b border-dashed border-primary">Forget Password ?</Link>
          </div>

          <div className="flex justify-center mb-6">
            <button
              className="btn w-full text-white bg-primary"
              type="submit"
              disabled={loading}
            >
              {loading ? "Signing in..." : "Log In"}
            </button>
          </div>
        </VerticalForm>
      </AuthLayout>
    </>
  )
}

export default Login
