import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

// form validation
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

// components
import { VerticalForm, FormInput } from "../../components";
import { saveSetupCredentials } from "../../modules/core/constants/setupKeys";
import { useSetup } from "../../modules/core/context/SetupContext";
import SetupLoadingScreen from "../../modules/core/components/SetupLoadingScreen";
import { setupService, SuperAdminSetupDto } from "../../modules/core/services/setupService";

interface SetupData {
  tenantName: string;
  domain: string;
  industry?: string;
  country?: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone?: string;
}

const SuperAdminSetup = () => {
  const navigate = useNavigate();
  const { isSetupComplete, isLoading, markSetupComplete, refreshSetupStatus } = useSetup();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const alreadyExists = isSetupComplete === true;

  useEffect(() => {
    void refreshSetupStatus();
  }, [refreshSetupStatus]);

  const redirectToLogin = () => {
    navigate("/auth/login", { replace: true });
  };

  const handleAlreadyExists = () => {
    markSetupComplete();
    redirectToLogin();
  };

  /*
   * form validation schema
   */
  const schemaResolver = yupResolver(
    yup.object().shape({
      tenantName: yup.string().required("Tenant name is required"),
      domain: yup.string()
        .required("Domain is required")
        .matches(/^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i, "Invalid domain format"),
      industry: yup.string(),
      country: yup.string(),
      firstName: yup.string().required("First name is required"),
      lastName: yup.string().required("Last name is required"),
      email: yup.string().email("Please enter a valid email").required("Email is required"),
      password: yup.string()
        .min(8, "Password must be at least 8 characters")
        .matches(/[A-Z]/, "Password must contain at least one uppercase letter")
        .matches(/[a-z]/, "Password must contain at least one lowercase letter")
        .matches(/[0-9]/, "Password must contain at least one number")
        .required("Password is required"),
      confirmPassword: yup.string()
        .oneOf([yup.ref("password")], "Passwords must match")
        .required("Please confirm your password"),
      phone: yup.string(),
    })
  );

  /*
   * handle form submission
   */
  const onSubmit = async (formData: SetupData) => {
    setError(null);
    setSuccess(null);
    setLoading(true);

    try {
      if (alreadyExists) {
        handleAlreadyExists();
        return;
      }

      const setupDto: SuperAdminSetupDto = {
        tenantName: formData.tenantName.trim(),
        domain: formData.domain.trim().toLowerCase(),
        industry: formData.industry?.trim() || undefined,
        country: formData.country?.trim() || undefined,
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        email: formData.email.trim().toLowerCase(),
        password: formData.password,
        phone: formData.phone?.trim() || undefined,
      };

      const response = await setupService.setupSuperAdmin(setupDto);

      if (response.success && response.tenantId && response.userId) {
        markSetupComplete();
        saveSetupCredentials({
          tenantId: response.tenantId,
          email: response.email || setupDto.email,
        });
        setSuccess("Super admin setup completed successfully! Redirecting to login...");
        const loginParams = new URLSearchParams({
          tenantId: response.tenantId,
          email: response.email || setupDto.email,
        });
        setTimeout(() => {
          navigate(`/auth/login?${loginParams.toString()}`);
        }, 1500);
      } else {
        setError(response.message || "Setup failed. Please try again.");
      }
    } catch (err: unknown) {
      const message =
        (err as { message?: string })?.message ||
        (err as { response?: { data?: { Error?: string; error?: string } } })?.response?.data?.Error ||
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ||
        "An error occurred during setup. Please try again.";

      if (/already exists/i.test(message)) {
        handleAlreadyExists();
        return;
      }

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  if (isLoading || isSetupComplete === null) {
    return <SetupLoadingScreen />;
  }

  if (alreadyExists) {
    return (
      <div className="bg-gradient-to-r from-rose-100 to-teal-100 dark:from-gray-700 dark:via-gray-900 dark:to-black min-h-screen">
        <div className="flex items-center justify-center min-h-screen py-12 px-4 sm:px-6 lg:px-8">
          <div className="max-w-md w-full">
            <div className="bg-white dark:bg-gray-800 shadow-xl rounded-xl overflow-hidden border border-gray-200 dark:border-gray-700">
              <div className="bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-900/20 dark:to-emerald-900/20 px-6 py-8 text-center">
                <div className="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-green-100 dark:bg-green-900 mb-4">
                  <svg className="h-8 w-8 text-green-600 dark:text-green-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">Setup Already Completed</h3>
                <p className="text-gray-600 dark:text-gray-400">
                  Super admin has already been configured for this system.
                </p>
              </div>
              <div className="p-6 bg-gray-50 dark:bg-gray-900/50">
                <div className="rounded-lg bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 p-4 mb-6">
                  <div className="flex">
                    <div className="flex-shrink-0">
                      <svg className="h-5 w-5 text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                      </svg>
                    </div>
                    <div className="ml-3">
                      <p className="text-sm text-blue-700 dark:text-blue-300">
                        Please use the login page to access the system with your super admin credentials.
                      </p>
                    </div>
                  </div>
                </div>
                <button
                  onClick={() => navigate("/auth/login")}
                  className="w-full inline-flex items-center justify-center px-6 py-3 border border-transparent text-base font-medium rounded-lg shadow-sm text-white bg-gradient-to-r from-primary to-primary/90 hover:from-primary/90 hover:to-primary focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary transition-all transform hover:scale-105"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                  </svg>
                  Go to Login Page
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-r from-rose-100 to-teal-100 dark:from-gray-700 dark:via-gray-900 dark:to-black min-h-screen">
      <div className="flex items-center justify-center min-h-screen py-8 px-4 sm:px-6 lg:px-8 xl:px-12">
        <div className="w-full max-w-5xl 2xl:max-w-6xl space-y-6">
          {/* Header Section */}
          <div className="text-center mb-6">
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 mb-4">
              <svg className="w-8 h-8 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
            <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
              Super Admin Setup
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400">
              Create the initial super administrator account for your ERP system
            </p>
            <div className="mt-4 inline-flex items-center px-4 py-2 rounded-full bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-300 text-sm">
              <svg className="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
              </svg>
              This setup can only be performed once
            </div>
          </div>

          {/* Alert Messages */}
          {error && (
            <div className="rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 p-4 shadow-sm">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800 dark:text-red-200">Error</h3>
                  <p className="mt-1 text-sm text-red-700 dark:text-red-300">{error}</p>
                </div>
              </div>
            </div>
          )}

          {success && (
            <div className="rounded-lg bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 p-4 shadow-sm">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-green-800 dark:text-green-200">Success</h3>
                  <p className="mt-1 text-sm text-green-700 dark:text-green-300">{success}</p>
                </div>
              </div>
            </div>
          )}

          {/* Main Form */}
          <VerticalForm<SetupData>
            onSubmit={onSubmit}
            resolver={schemaResolver}
            defaultValues={{
              tenantName: "",
              domain: "",
              industry: "",
              country: "",
              firstName: "",
              lastName: "",
              email: "",
              password: "",
              confirmPassword: "",
              phone: "",
            }}
          >
            <div className="bg-white dark:bg-gray-800 shadow-xl rounded-xl overflow-hidden border border-gray-200 dark:border-gray-700">
              {/* Tenant Information Section */}
              <div className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-blue-500 text-white">
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                      </svg>
                    </div>
                  </div>
                  <div className="ml-4">
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Tenant Information</h3>
                    <p className="text-sm text-gray-600 dark:text-gray-400">Configure your organization details</p>
                  </div>
                </div>
              </div>

              <div className="p-6">
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                  <FormInput
                    label="Tenant Name"
                    type="text"
                    name="tenantName"
                    placeholder="e.g., Acme Corporation"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Domain"
                    type="text"
                    name="domain"
                    placeholder="example.com"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Industry"
                    type="text"
                    name="industry"
                    placeholder="e.g., Technology, Healthcare"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Country"
                    type="text"
                    name="country"
                    placeholder="e.g., United States"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                </div>
              </div>

              {/* Divider */}
              <div className="px-6">
                <div className="border-t border-gray-200 dark:border-gray-700"></div>
              </div>

              {/* Super Admin Information Section */}
              <div className="bg-gradient-to-r from-purple-50 to-pink-50 dark:from-purple-900/20 dark:to-pink-900/20 px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-purple-500 text-white">
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                    </div>
                  </div>
                  <div className="ml-4">
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Super Admin Account</h3>
                    <p className="text-sm text-gray-600 dark:text-gray-400">Create your administrator credentials</p>
                  </div>
                </div>
              </div>

              <div className="p-6">
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                  <FormInput
                    label="First Name"
                    type="text"
                    name="firstName"
                    placeholder="John"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Last Name"
                    type="text"
                    name="lastName"
                    placeholder="Doe"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Email Address"
                    type="email"
                    name="email"
                    placeholder="admin@example.com"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <FormInput
                    label="Phone Number"
                    type="text"
                    name="phone"
                    placeholder="+1 (555) 123-4567"
                    containerClass="mb-0"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                  />
                  <div className="sm:col-span-2">
                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                      <FormInput
                        label="Password"
                        type="password"
                        name="password"
                        placeholder="Enter a strong password"
                        containerClass="mb-0"
                        className="form-input"
                        labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                      />
                      <FormInput
                        label="Confirm Password"
                        type="password"
                        name="confirmPassword"
                        placeholder="Confirm your password"
                        containerClass="mb-0"
                        className="form-input"
                        labelClassName="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                      />
                    </div>
                    <div className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                      Password must be at least 8 characters with uppercase, lowercase, and numbers
                    </div>
                  </div>
                </div>
              </div>

              {/* Footer Actions */}
              <div className="bg-gray-50 dark:bg-gray-900/50 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
                <div className="flex items-center justify-between">
                  <button
                    type="button"
                    onClick={() => navigate("/auth/login")}
                    className="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary transition-colors"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                    </svg>
                    Back to Login
                  </button>
                  <button
                    type="submit"
                    disabled={loading || alreadyExists}
                    className="inline-flex items-center px-6 py-3 border border-transparent text-base font-medium rounded-lg shadow-sm text-white bg-gradient-to-r from-primary to-primary/90 hover:from-primary/90 hover:to-primary focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary disabled:opacity-50 disabled:cursor-not-allowed transition-all transform hover:scale-105 disabled:transform-none"
                  >
                    {loading ? (
                      <>
                        <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Setting up...
                      </>
                    ) : (
                      <>
                        <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                        </svg>
                        Complete Setup
                      </>
                    )}
                  </button>
                </div>
              </div>
            </div>
          </VerticalForm>
        </div>
      </div>
    </div>
  );
};

export default SuperAdminSetup;

