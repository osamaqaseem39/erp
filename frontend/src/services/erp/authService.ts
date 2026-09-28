import { APICore } from "../../helpers/api/apiCore";
import type { LoginDto, AuthResponse, AuthResponseDto } from "../../types/erp";

const api = new APICore();

// Helper to normalize API response (handle both camelCase and PascalCase)
const normalizeAuthResponse = (data: any): AuthResponse => {
  // Handle both PascalCase (C# default) and camelCase (if configured)
  const token = data.Token || data.token || "";
  const refreshToken = data.RefreshToken || data.refreshToken || "";
  const expiresAt = data.ExpiresAt || data.expiresAt;
  const userData = data.User || data.user;

  // Normalize user object
  const user = userData ? {
    id: userData.Id || userData.id || "",
    username: userData.Username || userData.username || userData.PrimaryEmail || userData.primaryEmail || "",
    email: userData.PrimaryEmail || userData.primaryEmail || userData.Email || userData.email || "",
    firstName: userData.FirstName || userData.firstName,
    lastName: userData.LastName || userData.lastName,
    fullName: userData.FullName || userData.fullName,
    phoneNumber: userData.PrimaryPhone || userData.primaryPhone || userData.PhoneNumber || userData.phoneNumber,
    isActive: userData.IsActive ?? userData.isActive ?? true,
    isEmailVerified: userData.IsEmailVerified ?? userData.isEmailVerified ?? false,
    profilePicture: userData.ProfilePicture || userData.profilePicture,
    companyId: userData.CompanyId || userData.companyId,
    roleNames: userData.RoleNames || userData.roleNames || [],
  } : {
    id: "",
    username: "",
    email: "",
    isActive: false,
    isEmailVerified: false,
    roleNames: [],
  };

  return {
    token,
    refreshToken,
    expiresAt: expiresAt 
      ? (typeof expiresAt === 'string' ? expiresAt : new Date(expiresAt).toISOString())
      : new Date().toISOString(),
    user,
  };
};

export const authService = {
  login: async (credentials: LoginDto): Promise<AuthResponse> => {
    const response = await api.create("/auth/login", credentials);
    const data = normalizeAuthResponse(response.data as AuthResponseDto);
    
    if (data.token) {
      // Store a flat session object for compatibility with existing auth checks
      // PrivateRoute and other legacy code expect `user.token` and top-level user fields.
      api.setLoggedInUser({
        token: data.token,
        refreshToken: data.refreshToken,
        ...data.user,
      });
    }
    return data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    try {
      await api.create("/auth/logout", { refreshToken });
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      api.setLoggedInUser(null);
    }
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await api.create("/auth/refresh", { refreshToken });
    const data = normalizeAuthResponse(response.data as AuthResponseDto);
    
    if (data.token) {
      api.setLoggedInUser({
        token: data.token,
        refreshToken: data.refreshToken,
        ...data.user,
      });
    }
    return data;
  },
};

