import { APICore } from "../../helpers/api/apiCore";
import type { LoginDto, AuthResponse, AuthResponseDto } from "../../types/erp";

const api = new APICore();

/** Normalize API response (PascalCase or camelCase) and keep permissions on the session. */
const normalizeAuthResponse = (data: any): AuthResponse => {
  const token = data.Token || data.token || "";
  const refreshToken = data.RefreshToken || data.refreshToken || "";
  const expiresAt = data.ExpiresAt || data.expiresAt;
  const userData = data.User || data.user;
  const permissions: string[] = Array.isArray(data.Permissions)
    ? data.Permissions
    : Array.isArray(data.permissions)
      ? data.permissions
      : [];
  const isSuperAdmin = Boolean(data.IsSuperAdmin ?? data.isSuperAdmin);

  const user = userData
    ? {
        id: userData.Id || userData.id || "",
        username:
          userData.Username ||
          userData.username ||
          userData.PrimaryEmail ||
          userData.primaryEmail ||
          "",
        email:
          userData.PrimaryEmail ||
          userData.primaryEmail ||
          userData.Email ||
          userData.email ||
          "",
        firstName: userData.FirstName || userData.firstName,
        lastName: userData.LastName || userData.lastName,
        fullName: userData.FullName || userData.fullName,
        phoneNumber:
          userData.PrimaryPhone ||
          userData.primaryPhone ||
          userData.PhoneNumber ||
          userData.phoneNumber,
        isActive: userData.IsActive ?? userData.isActive ?? true,
        isEmailVerified: userData.IsEmailVerified ?? userData.isEmailVerified ?? false,
        profilePicture: userData.ProfilePicture || userData.profilePicture,
        companyId: userData.CompanyId || userData.companyId,
        roleNames: userData.RoleNames || userData.roleNames || [],
        permissions,
      }
    : {
        id: "",
        username: "",
        email: "",
        isActive: false,
        isEmailVerified: false,
        roleNames: [],
        permissions,
      };

  return {
    token,
    refreshToken,
    expiresAt: expiresAt
      ? typeof expiresAt === "string"
        ? expiresAt
        : new Date(expiresAt).toISOString()
      : new Date().toISOString(),
    user,
    permissions,
    isSuperAdmin,
  };
};

const persistSession = (data: AuthResponse) => {
  if (!data.token) return;
  // Flat session for PrivateRoute / permission checks: token + user fields + permissions.
  api.setLoggedInUser({
    token: data.token,
    refreshToken: data.refreshToken,
    ...data.user,
    permissions: data.permissions,
    isSuperAdmin: data.isSuperAdmin,
  });
};

export const authService = {
  login: async (credentials: LoginDto): Promise<AuthResponse> => {
    const response = await api.create("/auth/login", credentials);
    const data = normalizeAuthResponse(response.data as AuthResponseDto);
    persistSession(data);
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
    persistSession(data);
    return data;
  },

  /** Reload roles/permissions into the session (e.g. after role changes). */
  refreshSession: async (): Promise<AuthResponse | null> => {
    const current = api.getLoggedInUser() as { token?: string; refreshToken?: string } | null;
    if (!current?.token) return null;
    const response = await api.get("/auth/me", null);
    const me = response.data as any;
    const permissions: string[] = Array.isArray(me.Permissions)
      ? me.Permissions
      : Array.isArray(me.permissions)
        ? me.permissions
        : [];
    const isSuperAdmin = Boolean(me.IsSuperAdmin ?? me.isSuperAdmin);
    const userData = me.User || me.user;
    const roles = me.Roles || me.roles || userData?.RoleNames || userData?.roleNames || [];
    const data: AuthResponse = {
      token: current.token,
      refreshToken: current.refreshToken || "",
      expiresAt: new Date().toISOString(),
      permissions,
      isSuperAdmin,
      user: {
        id: userData?.Id || userData?.id || "",
        username: userData?.PrimaryEmail || userData?.primaryEmail || userData?.email || "",
        email: userData?.PrimaryEmail || userData?.primaryEmail || userData?.Email || userData?.email || "",
        firstName: userData?.FirstName || userData?.firstName,
        lastName: userData?.LastName || userData?.lastName,
        fullName: userData?.FullName || userData?.fullName,
        isActive: userData?.IsActive ?? userData?.isActive ?? true,
        isEmailVerified: userData?.IsEmailVerified ?? userData?.isEmailVerified ?? false,
        roleNames: roles,
        permissions,
      },
    };
    persistSession(data);
    return data;
  },
};
