import { client } from '../../lib/apolloClient';
import { SETUP_SUPER_ADMIN } from '../../graphql/mutations/setupMutations';
import { CHECK_SUPER_ADMIN_EXISTS } from '../../graphql/queries/setupQueries';

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

export const setupService = {
  checkSuperAdminExists: async (): Promise<boolean> => {
    try {
      const response = await client.query({
        query: CHECK_SUPER_ADMIN_EXISTS,
      });
      return response.data?.checkSuperAdminExists?.exists ?? false;
    } catch (error) {
      console.error('Error checking super admin exists:', error);
      return false;
    }
  },

  setupSuperAdmin: async (
    dto: SuperAdminSetupDto
  ): Promise<SuperAdminSetupResponseDto> => {
    try {
      const response = await client.mutate({
        mutation: SETUP_SUPER_ADMIN,
        variables: {
          tenantName: dto.tenantName,
          domain: dto.domain,
          industry: dto.industry,
          country: dto.country,
          firstName: dto.firstName,
          lastName: dto.lastName,
          email: dto.email,
          password: dto.password,
          phone: dto.phone,
        },
      });

      const data = response.data?.setupSuperAdmin;
      if (!data) {
        throw new Error('No response received from server');
      }

      return {
        success: data.success ?? true,
        message: data.message || 'Setup completed successfully',
        tenantId: data.tenantId,
        userId: data.userId,
        email: data.email,
      };
    } catch (error: unknown) {
      const err = error as { message?: string };
      const errorMessage = err.message || 'An error occurred during setup';
      throw new Error(errorMessage);
    }
  },
};
