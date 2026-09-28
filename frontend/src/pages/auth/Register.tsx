import { useEffect, useState } from 'react';
import * as yup from "yup";
import { yupResolver } from '@hookform/resolvers/yup';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';

// components
import { FormInput, VerticalForm, AuthLayout, PageBreadcrumb } from '../../components'

// redux
import { useDispatch } from 'react-redux';
import { AppDispatch } from '../../redux/store';
import { resetAuth } from '../../redux/actions';
// Use Core module services to avoid duplication
import { tenantService, type TenantDto } from '../../modules/core/entities/tenant/services';
import { userService, type CreateUserDto } from '../../modules/core/entities/user/services';
import { getTenantId, getTenantFromQuery, getTenantFromSubdomain } from '../../utils/tenant';

interface UserData {
  fullname: string;
  email: string;
  password: string;
  confirmPassword: string;
  tenantId?: string;
}

/* bottom links */
const BottomLink = () => {
  return (
    <p className="text-gray-500 dark:text-gray-400 text-center">Already have account ?
      <Link to="/auth/login" className="text-primary ms-1">
        <b>Log In</b>
      </Link>
    </p>
  );
};

const Register = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [tenants, setTenants] = useState<TenantDto[]>([]);
  const [selectedTenant, setSelectedTenant] = useState<TenantDto | null>(null);
  const [showTenantSelect, setShowTenantSelect] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingTenants, setLoadingTenants] = useState(true);
  const [tenantIdFromUrl, setTenantIdFromUrl] = useState<string | null>(null);

  useEffect(() => {
    dispatch(resetAuth() as any);
    
    // Try to get tenant ID from URL/subdomain
    const urlTenantId = getTenantFromQuery();
    const subdomainTenant = getTenantFromSubdomain();
    
    if (urlTenantId) {
      // If tenant ID is in URL, try to fetch tenant details
      setTenantIdFromUrl(urlTenantId);
      tenantService.getById(urlTenantId)
        .then((tenant) => {
          setSelectedTenant(tenant);
          setShowTenantSelect(false);
        })
        .catch(() => {
          // If tenant not found by ID, try to find by domain
          if (subdomainTenant) {
            loadTenantByDomain(subdomainTenant);
          } else {
            loadAllTenants();
          }
        });
    } else if (subdomainTenant) {
      // Try to find tenant by subdomain/domain
      loadTenantByDomain(subdomainTenant);
    } else {
      // No tenant info in URL, show tenant selection
      loadAllTenants();
    }
  }, [dispatch, searchParams.toString()]);

  const loadTenantByDomain = async (domain: string) => {
    try {
      const tenant = await tenantService.getByDomain(domain);
      setSelectedTenant(tenant);
      setTenantIdFromUrl(tenant.id);
      setShowTenantSelect(false);
    } catch (err) {
      console.error("Failed to load tenant by domain:", err);
      // Fallback to showing all tenants
      loadAllTenants();
    }
  };

  const loadAllTenants = async () => {
    setLoadingTenants(true);
    try {
      const data = await tenantService.getAll();
      console.log("Loaded tenants:", data);
      setTenants(data || []);
      
      // Always show tenant select if we're in this function
      setShowTenantSelect(true);
      
      // If only one tenant, auto-select it
      if (data && data.length === 1) {
        setSelectedTenant(data[0]);
        setTenantIdFromUrl(data[0].id);
        setShowTenantSelect(false); // Hide dropdown since tenant is auto-selected
      } else if (!data || data.length === 0) {
        // No tenants available, but still show the input field
        setShowTenantSelect(true);
        console.warn("No tenants found. User will need to enter tenant ID manually.");
      }
    } catch (err: any) {
      console.error("Failed to load tenants:", err);
      // Even on error, show tenant select so user can manually enter
      setShowTenantSelect(true);
      setTenants([]);
      // Don't show error to user - they can still enter tenant ID manually
    } finally {
      setLoadingTenants(false);
    }
  };

  // Handle tenant selection from dropdown
  const handleTenantChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const tenantId = (e.target as HTMLSelectElement).value;
    if (!tenantId) {
      setSelectedTenant(null);
      return;
    }
    
    const tenant = tenants.find(t => t.id === tenantId);
    if (tenant) {
      setSelectedTenant(tenant);
      setTenantIdFromUrl(tenant.id);
    }
  };

  /*
   * form validation schema
   * Business logic rules matching backend validation:
   * - FirstName/LastName: Required, max 100 characters
   * - Email: Required, valid format, max 100 characters
   * - Password: Required, minimum 8 characters, max 100 characters
   * - TenantId: Required if not provided via URL/subdomain
   */
  const schemaResolver = yupResolver(
    yup.object().shape({
      fullname: yup
        .string()
        .required("Please enter Full Name")
        .test("has-first-last", "Please enter both first and last name", (value) => {
          if (!value) return false;
          const parts = value.trim().split(/\s+/);
          return parts.length >= 2 && parts[0].length > 0 && parts.slice(1).join(" ").length > 0;
        })
        .test("max-length", "Full name cannot exceed 200 characters", (value) => {
          return !value || value.length <= 200;
        }),
      email: yup
        .string()
        .required("Please enter Email")
        .email("Please enter a valid email address")
        .max(100, "Email cannot exceed 100 characters"),
      password: yup
        .string()
        .required("Please enter Password")
        .min(8, "Password must be at least 8 characters")
        .max(100, "Password cannot exceed 100 characters"),
      confirmPassword: yup
        .string()
        .required("Please confirm Password")
        .oneOf([yup.ref("password")], "Passwords must match"),
      tenantId: yup.string().when([], {
        is: () => !tenantIdFromUrl && !selectedTenant,
        then: (schema) => schema.required("Tenant is required"),
        otherwise: (schema) => schema.optional(),
      }),
    })
  );

  /*
 * handle form submission
 */
  const onSubmit = async (formData: UserData) => {
    setError(null);
    setLoading(true);
    
    try {
      // Determine tenant ID from multiple sources (priority order)
      let tenantId: string | null = null;
      let finalTenant: TenantDto | null = selectedTenant;
      
      // Priority 1: Tenant from URL/subdomain (already loaded)
      if (tenantIdFromUrl && selectedTenant) {
        tenantId = selectedTenant.id;
        finalTenant = selectedTenant;
      }
      // Priority 2: Selected tenant from state
      else if (selectedTenant?.id) {
        tenantId = selectedTenant.id;
        finalTenant = selectedTenant;
      }
      // Priority 3: Tenant ID from form data
      else if (formData.tenantId && formData.tenantId.trim()) {
        const formTenantId = formData.tenantId.trim();
        
        // Check if it's a valid GUID format
        const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
        if (guidRegex.test(formTenantId)) {
          // It's a GUID - try to fetch the tenant
          try {
            const tenant = await tenantService.getById(formTenantId);
            tenantId = tenant.id;
            finalTenant = tenant;
            setSelectedTenant(tenant);
          } catch (error) {
            setError(`Tenant with ID ${formTenantId} not found. Please select a tenant from the dropdown.`);
            setLoading(false);
            return;
          }
        } else {
          // It might be a domain - try to look it up
          try {
            const tenant = await tenantService.getByDomain(formTenantId);
            tenantId = tenant.id;
            finalTenant = tenant;
            setSelectedTenant(tenant);
          } catch (domainError) {
            setError(`Tenant "${formTenantId}" not found. Please select a tenant from the dropdown.`);
            setLoading(false);
            return;
          }
        }
      }
      
      // Final validation
      if (!tenantId) {
        if (tenants.length > 0) {
          setError("Please select a tenant from the dropdown above.");
        } else {
          setError("No tenants available. Please contact your administrator or provide a valid tenant ID or domain.");
        }
        setLoading(false);
        return;
      }
      
      // Ensure we have a valid GUID format
      const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
      if (!guidRegex.test(tenantId)) {
        setError("Invalid tenant ID format. Please select a tenant from the dropdown.");
        setLoading(false);
        return;
      }

      // Business Logic: Split fullname into firstName and lastName
      // Validate that we have both first and last name
      const nameParts = formData.fullname.trim().split(/\s+/);
      const firstName = nameParts[0]?.trim() || "";
      const lastName = nameParts.slice(1).join(" ").trim() || "";

      // Business Logic: Validate name parts
      if (!firstName || firstName.length === 0) {
        setError("First name is required");
        setLoading(false);
        return;
      }
      if (firstName.length > 100) {
        setError("First name cannot exceed 100 characters");
        setLoading(false);
        return;
      }
      if (!lastName || lastName.length === 0) {
        setError("Last name is required");
        setLoading(false);
        return;
      }
      if (lastName.length > 100) {
        setError("Last name cannot exceed 100 characters");
        setLoading(false);
        return;
      }

      // Business Logic: Validate email format and length
      if (formData.email.length > 100) {
        setError("Email cannot exceed 100 characters");
        setLoading(false);
        return;
      }

      // Business Logic: Validate password strength
      if (formData.password.length < 8) {
        setError("Password must be at least 8 characters");
        setLoading(false);
        return;
      }
      if (formData.password.length > 100) {
        setError("Password cannot exceed 100 characters");
        setLoading(false);
        return;
      }

      // Prepare CreateUserDto according to API structure (Admin.CreateUserDto)
      // Business Logic: Admin.CreateUserDto uses: FirstName, LastName, Email, Password, Phone, IsActive, RoleIds
      // Note: RoleIds can be empty for registration - roles will be assigned by admin later
      const createUserDto: CreateUserDto = {
        firstName: firstName,
        lastName: lastName,
        email: formData.email.trim().toLowerCase(), // Normalize email
        password: formData.password,
        phone: "", // Optional - can be added to form if needed
        isActive: true, // New users are active by default, but may need admin approval
        roleIds: [], // Empty array for registration - admin will assign roles
      };

      // Business Logic: Call the user service to create the user
      // Note: This endpoint requires authorization, so it may fail with 401
      // The tenantId will be sent in X-Tenant-Id header for tenant context
      try {
        const userId = await userService.create(createUserDto, tenantId);
        
        // Business Logic: Success - user created
        // Show success message and redirect to login
        setError(null);
        const tenantName = selectedTenant?.name || "your organization";
        alert(`Registration successful! Your account has been created for ${tenantName}. Please contact your administrator to activate your account and assign roles.`);
        navigate("/auth/login");
      } catch (apiError: any) {
        // Business Logic: Handle API errors according to backend responses
        if (apiError.response?.status === 401) {
          // Unauthorized - endpoint requires authentication
          const tenantName = selectedTenant?.name || "your organization";
          setError(`Registration for ${tenantName} requires administrator approval. Please contact your system administrator to create an account.`);
        } else if (apiError.response?.status === 400) {
          // Bad Request - validation errors from backend
          const errorData = apiError.response?.data;
          if (errorData?.errors) {
            // FluentValidation errors format
            const errorMessages = Object.values(errorData.errors).flat().join(", ");
            setError(errorMessages);
          } else {
            setError(errorData?.error || errorData?.message || "Validation failed. Please check your input.");
          }
        } else if (apiError.response?.status === 409) {
          // Conflict - user already exists
          setError("An account with this email already exists. Please use a different email or contact support.");
        } else {
          // Other errors
          const errorMessage = apiError.response?.data?.error 
            || apiError.response?.data?.message 
            || apiError.message 
            || "Registration failed. Please try again.";
          setError(errorMessage);
        }
      }
      
    } catch (err: any) {
      const errorMessage = err.response?.data?.message 
        || err.response?.data?.error 
        || err.message 
        || "Registration failed. Please try again.";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title='Register' />
      <AuthLayout
        authTitle='Sign Up'
        helpText="Request an account. Your registration will be reviewed by an administrator."
        bottomLinks={<BottomLink />}
        hasThirdPartyLogin={false}
      >
        {error && (
          <div className="alert alert-warning mb-4" role="alert">
            {error}
          </div>
        )}

        <VerticalForm<UserData>
          onSubmit={onSubmit}
          resolver={schemaResolver}
          defaultValues={{ 
            fullname: "", 
            email: "", 
            password: "", 
            confirmPassword: "", 
            tenantId: tenantIdFromUrl || selectedTenant?.id || "" 
          }}
        >
          {loadingTenants ? (
            <div className="mb-4 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400">Loading tenants...</p>
            </div>
          ) : selectedTenant ? (
            <div className="mb-4 p-3 bg-info/10 border border-info/20 rounded-lg">
              <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Registering for:</p>
              <p className="font-semibold text-info">{selectedTenant.name}</p>
              <p className="text-xs text-gray-500 dark:text-gray-500">{selectedTenant.domain}</p>
              <input type="hidden" name="tenantId" value={selectedTenant.id} />
            </div>
          ) : showTenantSelect && tenants.length > 0 ? (
            <FormInput
              label="Select Tenant"
              type="select"
              name="tenantId"
              containerClass="mb-4"
              className="form-select"
              labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              required
              onChange={handleTenantChange}
            >
              <option value="">Select a tenant</option>
              {tenants.map((tenant) => (
                <option key={tenant.id} value={tenant.id}>
                  {tenant.name} ({tenant.domain})
                </option>
              ))}
            </FormInput>
          ) : (
            <div className="mb-4">
              <FormInput
                label="Tenant ID or Domain"
                type="text"
                name="tenantId"
                placeholder="Enter tenant ID (GUID) or domain (e.g., company1)"
                containerClass="mb-2"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />
              {tenants.length === 0 && !loadingTenants && (
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  No tenants available. Please enter a tenant ID or domain manually.
                </p>
              )}
            </div>
          )}

          <FormInput
            label='Full Name'
            type='text'
            name='fullname'
            placeholder='Enter Full Name'
            containerClass='mb-4'
            className='form-input'
            labelClassName='block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2'
            required
          />

          <FormInput
            label='Email Address'
            type='email'
            name='email'
            placeholder='Enter your Email'
            containerClass='mb-4'
            className='form-input'
            labelClassName='block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2'
            required
          />

          <FormInput
            label='Password'
            type='password'
            name='password'
            placeholder='Enter your password'
            containerClass='mb-4'
            className='form-input'
            labelClassName='block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2'
            required
          />

          <FormInput
            label='Confirm Password'
            type='password'
            name='confirmPassword'
            placeholder='Confirm your password'
            containerClass='mb-4'
            className='form-input'
            labelClassName='block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2'
            required
          />

          <div className="mb-4">
            <FormInput
              label='I accept'
              type='checkbox'
              name='checkbox'
              containerClass='flex items-center'
              labelClassName='ms-2 text-slate-900 dark:text-slate-200'
              className='form-checkbox rounded'
              otherComp={<a href="" target='_blank' className="text-gray-400 underline">Terms and Conditions</a>}
            />
          </div>

          <div className="flex justify-center mb-6">
            <button type='submit' className="btn w-full text-white bg-primary" disabled={loading}>
              {loading ? "Submitting..." : "Request Account"}
            </button>
          </div>

        </VerticalForm>
      </AuthLayout>
    </>
  )
}

export default Register