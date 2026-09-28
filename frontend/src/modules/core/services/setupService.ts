export interface SuperAdminSetupDto {
  tenantName: string;
  domain: string;
  industry?: string;
  country?: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone?: string;
}

export interface SuperAdminSetupResponseDto {
  success: boolean;
  message: string;
  tenantId?: string;
  userId?: string;
  email?: string;
}

// Using REST API for setup endpoints (they have [AllowAnonymous])
// GraphQL requires authentication globally, so REST is better for public setup
const API_BASE_URL = "http://localhost:5252";

export const setupService = {
  checkSuperAdminExists: async (): Promise<boolean> => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/setup/super-admin/exists`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        console.error("Failed to check super admin exists:", response.statusText);
        return false;
      }

      const data = await response.json();
      return Boolean(data.exists ?? data.Exists ?? false);
    } catch (error) {
      console.error("Error checking super admin exists:", error);
      return false;
    }
  },

  setupSuperAdmin: async (dto: SuperAdminSetupDto): Promise<SuperAdminSetupResponseDto> => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/setup/super-admin`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dto),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        const errorMessage =
          errorData?.Error ||
          errorData?.error ||
          errorData?.message ||
          errorData?.Message ||
          response.statusText ||
          "Setup failed";
        throw new Error(errorMessage);
      }

      const data = await response.json();
      return {
        success: data.Success ?? data.success ?? true,
        message: data.Message || data.message || "Setup completed successfully",
        tenantId: data.TenantId || data.tenantId,
        userId: data.UserId || data.userId,
        email: data.Email || data.email,
      };
    } catch (error: unknown) {
      const err = error as { message?: string };
      const errorMessage = err.message || "An error occurred during setup";
      throw new Error(errorMessage);
    }
  },
};
